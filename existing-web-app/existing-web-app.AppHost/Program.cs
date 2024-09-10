var builder = DistributedApplication.CreateBuilder(args);

//var blobs = builder
//                    .AddAzureStorage("storage")
//                    .RunAsEmulator()
//                    .AddBlobs("blobs");

builder.AddProject<Projects.BlazorWebApp>("blazorwebapp")
    //.WithReference(blobs)
    ;

builder.Build().Run();
