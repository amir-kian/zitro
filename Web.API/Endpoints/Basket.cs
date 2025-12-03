using Application.Basket.AddToBasket;
using Application.DTOs;
using Application.Payment.StartPayment;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.API.Endpoints
{
    public class Basket : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("basket/add", async (
                [FromBody] AddToBasketRequest request,
                ISender sender) =>
            {
                var command = new AddToBasketCommand(
                    request.UserId,
                    request.ProductId
                    );

                var result = await sender.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new { error = result.ErrorMessage });
                }

                return Results.Ok(new { message = "Product added to basket successfully" });
            });

            app.MapPost("payment/start", async (
                [FromBody] StartPaymentRequest request,
                ISender sender) =>
            {
                var command = new StartPaymentCommand(request.UserId);
                var result = await sender.Send(command);

                if (!result.Success)
                {
                    return Results.BadRequest(new { error = result.ErrorMessage });
                }

                return Results.Ok(new { paymentId = result.PaymentId, message = "Payment process started" });
            });
        }
    }
}

