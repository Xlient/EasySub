using BlazorApp1.Services;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;

var builder = WebApplication.CreateBuilder(args);
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<VideoAnnoatationService>();
builder.Services.AddSingleton<CloudUploadService>();
builder.Services.AddSingleton<TranslationService>();
builder.Services.AddBlazorise(options =>
{
    options.ChangeTextOnKeyPress = true; // optional
})
      .AddBootstrapProviders()
      .AddFontAwesomeIcons();
//builder.Services.AddAntDesign();
builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(name: MyAllowSpecificOrigins,
            builder =>

        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseCors();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
