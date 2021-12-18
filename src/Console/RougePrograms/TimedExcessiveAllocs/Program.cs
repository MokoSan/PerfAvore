while(true)
{
    // LOH Alloc.
    byte[] alloc1 = new byte[1024 * 1024 * 1024];
    System.Console.WriteLine($"Allocating a byte array of size: {1024 * 1024 * 1024}");

    // SOH Alloc.
    byte[] alloc2 = new byte[1024 * 80];
    System.Console.WriteLine($"Allocating a byte array of size: {1024 * 80}");

    System.Threading.Thread.Sleep(500);
}