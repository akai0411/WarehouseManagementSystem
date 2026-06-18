namespace WarehouseManagementSystem.Api.Middleware
{
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
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        message = "Something went wrong",
                        detail = ex.Message
                    })
                );
            }
        }
    }
}
