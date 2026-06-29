

namespace TestBlazor_FNCourse.Data.Services
{
    public class EmailService
    {
        public void SendEmail(string to, string subject, string body)
        {
            // Simulate sending an email (in a real application, integrate with an email provider)
            Console.WriteLine($"Sending email to: {to}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {body}");
        }

        
    }
}
