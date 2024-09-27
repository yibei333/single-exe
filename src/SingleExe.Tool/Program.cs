using CliFx;

var builder = new CliApplicationBuilder();
builder.AddCommandsFromThisAssembly();
builder.SetExecutableName("single-exe");

var app = builder.Build();
await app.RunAsync();