using Franz.Domain.Entities;
using Franz.Domain.ValueObjects;

namespace Franz.Persistence.Seeders
{
    public static class MemberSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            // Delete existing records
            if (db.Members.Any())
            {
                db.Members.RemoveRange(db.Members);
                db.SaveChanges();
            }

            // Add new records
            db.Members.AddRange(
                new Member(new FullName("John Doe"), new Email("john.doe@example.com"))
                {
                   
                  
                },
                new Member(new FullName("Jane Smith"), new Email("jane.smith@example.com"))
                {
                    
                  
                },
                new Member(new FullName("Alice Johnson"), new Email("alice.j@example.com"))
                {
                    
                 
                },
                new Member(new FullName("Bob Roberts"), new Email("bob.r@example.com"))
                {
                   
                   
                },
                new Member(new FullName("Charlie Brown"), new Email("charlie.brown@example.com"))
                {
                   
                    
                },
                new Member(new FullName("Diana Prince"), new Email("diana.prince@example.com"))
                {
                  
                   
                }
            );

            db.SaveChanges();
        }
    }
}
