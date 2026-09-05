namespace Shopizy.NotificationService.Domain.Services;

public static class NotificationTemplateEngine
{
    public static (string Subject, string Body) FormatOrderConfirmation(Guid orderId, decimal totalAmount, string currency = "USD")
    {
        var subject = $"Shopizy Order Confirmation - #{orderId.ToString()[..8].ToUpperInvariant()}";
        var body = $"Thank you for your order! Your order #{orderId} for {totalAmount:C} {currency} has been received and is being processed.";
        return (subject, body);
    }

    public static (string Subject, string Body) FormatShipmentDispatched(Guid orderId, string carrier, string trackingNumber)
    {
        var subject = $"Your Shopizy Order #{orderId.ToString()[..8].ToUpperInvariant()} Has Shipped!";
        var trackingUrl = $"https://shopizy.com/track/{trackingNumber}";
        var body = $"Great news! Your package has been dispatched via {carrier}. Track your live shipment progression at {trackingUrl}";
        return (subject, body);
    }

    public static (string Subject, string Body) FormatPasswordReset(string resetToken)
    {
        var subject = "Shopizy Security Alert: Password Reset Request";
        var resetUrl = $"https://shopizy.com/reset-password?token={resetToken}";
        var body = $"A password reset was requested for your account. Click the following secure link to reset your password: {resetUrl}. This link expires in 15 minutes.";
        return (subject, body);
    }

    public static (string Subject, string Body) FormatBackInStock(string productName, decimal currentPrice)
    {
        var subject = $"Good news! {productName} is back in stock!";
        var body = $"An item on your wishlist, '{productName}', is now back in stock for {currentPrice:C}. Order now before inventory runs out!";
        return (subject, body);
    }
}
