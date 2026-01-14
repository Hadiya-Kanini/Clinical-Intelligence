using Microsoft.EntityFrameworkCore;

namespace ClinicalIntelligence.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
}
