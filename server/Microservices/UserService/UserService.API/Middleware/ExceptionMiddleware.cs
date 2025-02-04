using Microsoft.AspNetCore.Mvc;
using UserService.Application.Exceptions;

namespace UserService.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
      
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var problemDetails = new ProblemDetails
        {
            Title = "An error occurred while processing your request.",
            Status = ex switch
            {
                InvalidTokenException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                NotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            },
            Detail = ex.Message,
            Instance = context.Request.Path
        };
        
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status.Value;

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}