namespace AppDb;

using MessageModel;
using RoomModel;
using UserModel;

using DirectChatRoomModel;
using DirectMessageModel;


using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Message> Messages => Set<Message>();

    public DbSet<DirectMessage> DirectMessages => Set<DirectMessage>();
    public DbSet<DirectChatRoom> DirectRooms => Set<DirectChatRoom>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(t => t.UserName)
            .IsUnique();
        
        
        modelBuilder.Entity<Message>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Room)
            .WithMany()
            .HasForeignKey(m => m.RoomId)
            .OnDelete(DeleteBehavior.Cascade);
        


        modelBuilder.Entity<DirectMessage>()
            .HasOne(t => t.Sender)
            .WithMany()
            .HasForeignKey(t => t.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<DirectMessage>()
            .HasOne(t => t.Receiver)
            .WithMany()
            .HasForeignKey(t => t.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        

        modelBuilder.Entity<DirectChatRoom>()
            .HasOne(d => d.UserOne)
            .WithMany()
            .HasForeignKey(d => d.UserIdOne)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<DirectChatRoom>()
            .HasOne(d => d.UserTwo)
            .WithMany()
            .HasForeignKey(d => d.UserIdTwo)
            .OnDelete(DeleteBehavior.Restrict);
        

    }
}