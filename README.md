# Zitro Cart Challenge

سیستم فروشگاهی ساده با قابلیت سبد خرید، قفل محصولات و پردازش پرداخت از طریق RabbitMQ

## ویژگی‌ها

- ✅ افزودن محصول به سبد خرید با قفل ۱۰ دقیقه‌ای
- ✅ پردازش پرداخت از طریق RabbitMQ
- ✅ مدیریت سبد خرید با Redis
- ✅ قفل خودکار محصولات با TTL ۱۰ دقیقه
- ✅ مشاهده وضعیت محصولات (فروخته شده، قفل شده)

## پیش‌نیازها

- .NET 8.0 SDK
- SQL Server (یا PostgreSQL)
- Redis
- RabbitMQ

## نصب و راه‌اندازی

### 1. نصب Redis

**Windows:**
```bash
# استفاده از Docker
docker run -d -p 6379:6379 redis:latest
```

**Linux/Mac:**
```bash
# استفاده از Docker
docker run -d -p 6379:6379 redis:latest
```

### 2. نصب RabbitMQ

**Windows:**
```bash
# استفاده از Docker
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

**Linux/Mac:**
```bash
# استفاده از Docker
docker run -d -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 3. تنظیمات Connection Strings

فایل `Web.API/appsettings.json` را ویرایش کنید:

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=MyDb;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=True;",
    "Redis": "localhost:6379",
    "RabbitMQ": "amqp://guest:guest@localhost:5672"
  }
}
```

### 4. اجرای Migration

```bash
cd Web.API
dotnet ef database update --project ../Persistance
```

### 5. اجرای پروژه

```bash
cd Web.API
dotnet run
```

API در آدرس `https://localhost:5001` یا `http://localhost:5000` در دسترس خواهد بود.

Swagger UI: `https://localhost:5001/swagger`

## API Endpoints

### 1. مشاهده لیست محصولات

**GET** `/products`

نمایش لیست تمام محصولات با وضعیت قفل و فروش

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Product 1",
    "sku": "SKU001",
    "currency": "USD",
    "amount": 100.00,
    "isSold": false,
    "isLocked": true,
    "lockedBy": "user123"
  }
]
```

### 2. افزودن محصول به سبد خرید

**POST** `/basket/add`

افزودن محصول به سبد خرید با قفل ۱۰ دقیقه‌ای

**Request Body:**
```json
{
  "userId": "user123",
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "quantity": 1
}
```

**Response (Success):**
```json
{
  "message": "Product added to basket successfully"
}
```

**Response (Error):**
```json
{
  "error": "Product is locked by another user"
}
```

### 3. شروع فرآیند پرداخت

**POST** `/payment/start`

شروع فرآیند پرداخت برای سبد خرید (ارسال پیام به RabbitMQ)

**Request Body:**
```json
{
  "userId": "user123"
}
```

**Response (Success):**
```json
{
  "paymentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "message": "Payment process started"
}
```

**Response (Error):**
```json
{
  "error": "Basket is empty"
}
```

## نمونه درخواست‌ها

### مثال کامل: خرید محصول

```bash
# 1. مشاهده محصولات
curl -X GET https://localhost:5001/products

# 2. افزودن محصول به سبد خرید
curl -X POST https://localhost:5001/basket/add \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123",
    "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "quantity": 1
  }'

# 3. شروع پرداخت
curl -X POST https://localhost:5001/payment/start \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "user123"
  }'
```

## معماری

پروژه از معماری Clean DDD استفاده می‌کند:

- **Domain**: موجودیت‌های دامنه (Product, Basket)
- **Application**: منطق کسب‌وکار و Commands/Queries
- **Persistence**: دسترسی به داده (Entity Framework)
- **Web.API**: API Endpoints و Background Services

## جریان کار

1. **افزودن به سبد خرید:**
   - کاربر محصول را به سبد اضافه می‌کند
   - محصول برای ۱۰ دقیقه قفل می‌شود (Redis با TTL)
   - سبد خرید در Redis ذخیره می‌شود

2. **شروع پرداخت:**
   - پیام پرداخت به RabbitMQ ارسال می‌شود
   - BackgroundService پیام را دریافت می‌کند

3. **پردازش پرداخت:**
   - پرداخت به صورت تصادفی تایید یا رد می‌شود (شبیه‌سازی)
   - در صورت موفقیت:
     - محصول به عنوان فروخته شده علامت‌گذاری می‌شود
     - قفل محصول آزاد می‌شود
     - سبد خرید از Redis حذف می‌شود
   - در صورت عدم موفقیت:
     - قفل پس از ۱۰ دقیقه به صورت خودکار آزاد می‌شود (TTL)

## Redis Keys

- `Basket:{userId}`: سبد خرید کاربر
- `Lock:{productId}`: قفل محصول (TTL: 10 دقیقه)

## RabbitMQ

- **Exchange**: `payment_exchange`
- **Queue**: `payment_queue`
- **Routing Key**: `payment_queue`

## نکات مهم

- قفل محصولات به صورت خودکار پس از ۱۰ دقیقه آزاد می‌شود (TTL)
- پرداخت به صورت تصادفی شبیه‌سازی شده است (۵۰% موفقیت)
- در محیط production باید احراز هویت و مدیریت خطا اضافه شود

## توسعه‌دهندگان

این پروژه برای چالش Zitro Cart طراحی شده است.

