using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CMCS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

public sealed class TestFixture : IDisposable
{
    public AppDbContext Db { get; }
    public string TempRoot { get; }
    public IWebHostEnvironment Env { get; }

    public TestFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Db = new AppDbContext(options);
        Db.Database.EnsureCreated();

        TempRoot = Path.Combine(Path.GetTempPath(), "cmcs-tests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempRoot);
        Env = new FakeEnv(TempRoot);
    }

    public void Dispose()
    {
        Db.Dispose();
        try { Directory.Delete(TempRoot, true); } catch { }
    }

    private sealed class FakeEnv : IWebHostEnvironment
    {
        public FakeEnv(string root) { WebRootPath = root; ContentRootPath = root; }
        public string ApplicationName { get; set; } = "CMCS.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new PhysicalFileProvider("/");
        public string WebRootPath { get; set; }
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider("/");
    }
}
