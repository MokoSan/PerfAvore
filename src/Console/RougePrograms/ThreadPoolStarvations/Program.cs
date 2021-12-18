using System.Threading.Tasks;
using System.Threading;

while (true)
{
    Task.Run(() => {
        System.Console.WriteLine("Before Wait.");
        Task.Delay(2000).Wait();
        System.Console.WriteLine("Before After.");
    });

    // Rate of creating task < Rate of waits.
    Task.Delay(500);
}
