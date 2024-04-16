using System.Text;

namespace Tests.ShaktiTanwar;

[TestClass]
[DeploymentItem("ShaktiTanwar\\Test.txt")]
public class AsynchronousProgrammingModelTests
{
    [TestMethod]
    public async Task sync_read_file()
    {
        string path = @"Test.txt";

        using (FileStream fs = File.OpenRead(path))
        {
            byte[] b = new byte[1024];
            UTF8Encoding encoder = new UTF8Encoding(true);
            fs.Read(b, 0, b.Length);
            Console.WriteLine(encoder.GetString(b));
        }
    }
    
    [TestMethod]
    public async Task read_file_apm()
    {
        string filePath = @"Test.txt";

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous))
        {
            byte[] buffer = new byte[1024];
            UTF8Encoding encoder = new UTF8Encoding(true);
            IAsyncResult result = fs.BeginRead(buffer, 0, buffer.Length, null, null);

            Console.WriteLine("Do Something here");

            int numBytes = fs.EndRead(result);
            fs.Close();
            Console.WriteLine(encoder.GetString(buffer));
        }
    }
    
    [TestMethod]
    public async Task read_file_apm_task()
    {
        string filePath = @"Test.txt";
            
        //Open the stream and read content.
        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1024, FileOptions.Asynchronous))
        {
            byte[] buffer = new byte[1024];
            UTF8Encoding encoder = new UTF8Encoding(true);

            var task = Task<int>.Factory.FromAsync(fs.BeginRead, fs.EndRead, buffer, 0, buffer.Length,null);
            Console.WriteLine("Do Something while file is read asynchronously");
            task.Wait();
            Console.WriteLine(encoder.GetString(buffer));
        }
    }
}