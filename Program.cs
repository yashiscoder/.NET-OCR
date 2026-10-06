using OCR_DotNet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IImagePreprocessor, ImagePreprocessor>();
builder.Services.AddScoped<IMarksheetParser, MarksheetParser>();
builder.Services.AddScoped<ITesseractDataService, TesseractDataService>();
builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();
