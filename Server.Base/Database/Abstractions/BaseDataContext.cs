using Microsoft.EntityFrameworkCore;

namespace Server.Base.Database.Abstractions;
public abstract class BaseDataContext(DbContextOptions options) : DbContext(options) { }
