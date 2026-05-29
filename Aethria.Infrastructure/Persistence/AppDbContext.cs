using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Reflection;

namespace Aethria.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mentor> Mentors => Set<Mentor>();
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Roadmap> Roadmaps => Set<Roadmap>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizVersion> QuizVersions => Set<QuizVersion>();
    public DbSet<QuestionSnapshot> QuestionSnapshots => Set<QuestionSnapshot>();
    public DbSet<QuestionOptionSnapshot> QuestionOptionSnapshots => Set<QuestionOptionSnapshot>();
    public DbSet<QuizSubmission> QuizSubmissions => Set<QuizSubmission>();
    public DbSet<SubmissionAnswer> SubmissionAnswers => Set<SubmissionAnswer>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ResourceChunk> ResourceChunks => Set<ResourceChunk>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
