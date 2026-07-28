using Npgsql;
using System.Data;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});



// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseDefaultFiles();   // "/" -> index.html
app.UseStaticFiles();


app.UseCors("AllowAll");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/stock", (string p) =>
{
    List<Stock> stocuri = new List<Stock>();
    DataTable dt = new DataTable();
    using (NpgsqlConnection con = new NpgsqlConnection("Server=89.33.25.108;Port=5432;Database=postgres;User Id=postgres;Password=q6I26g8782sUPWpa5S2MeuFPm2WwyYIk;"))
    {
        con.Open();
        using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT stocuri.upid, stocuri.denumire, ((SELECT SUM(gp.stoc) FROM gestprod gp WHERE gp.upid = stocuri.upid)) as stoc FROM STOCKCHECK_ITEMS LEFT JOIN STOCKCHECK_ACC ON STOCKCHECK_ITEMS.acc = STOCKCHECK_ACC.id LEFT JOIN STOCURI ON STOCKCHECK_ITEMS.upid = STOCURI.upid WHERE STOCKCHECK_ACC.code = @p", con))
        {
            cmd.Parameters.AddWithValue("@p", p);
            using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
            {
                da.Fill(dt);
            }
        }
    
        if (dt.Rows.Count == 0)
        {
            return null;
        }
        foreach (DataRow row in dt.Rows)
        {
            stocuri.Add(new Stock(row["upid"].ToString(), row["denumire"].ToString(), row["stoc"].ToString()));
        }
    }
    return stocuri;
})
.WithName("GetStock")
.WithOpenApi();

app.Run();

class Stock
{
    public string upid { get; set; }
    public string denprod { get; set; }
    public string stoc { get; set; }

    public Stock(string upid, string denprod, string stoc)
    {
        this.upid = upid;
        this.denprod = denprod;
        this.stoc = stoc;
    }
}
