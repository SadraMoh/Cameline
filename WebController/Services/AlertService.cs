namespace WebController.Services
{
    public class AlertService
    {
        public void Error(Exception ex)
        {
            Console.WriteLine($"[ERR] : ({ex.GetType().FullName}) - {ex.Message}");
        }

        public void Error(string errMsg)
        {
            Console.WriteLine($"[ERR] - {errMsg}");
        }

        public void Notify(params string[] msg)
        {
            Console.WriteLine($"[MSG] - {string.Join(", ", msg)}");
        }

        public void Event(string eventName, params string[] msg)
        {
            Console.WriteLine($"[EVENT] : ({eventName}) - {string.Join(", ", msg)}");
        }
    }
}
