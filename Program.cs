using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Specialized;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;


namespace EntityFrameworkCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello Entity Framework!");
            Random r = new Random();

            using (var db = new FootballDBContext())
            {
                //Team Data input
                
                string a;
                do
                {
                    Console.WriteLine("Provide number of team (min value = 5)");
                    a = Console.ReadLine();
                    if (Convert.ToInt32(a) >= 5)
                    {
                        break;
                    }
                } while (true);

                for (int i = 0; i < Convert.ToInt32(a); i++)
                {
                    Console.WriteLine("Enter Name of Team " + (i+1));
                    string tempTeamName = Console.ReadLine();
                    db.Add(new Team
                    {
                        Name = tempTeamName,
                        Points = 0
                    });
                }

                db.SaveChanges();

                var teamIDList = db.Teams.Select(t => t.TeamID).ToList();

                //Game Generation

                var matchMaking = from item1 in teamIDList
                                   from item2 in teamIDList
                                   where item1 < item2
                                   select Tuple.Create(item1, item2);

                foreach (var g in matchMaking){
                    db.Add(new Game
                    {
                        HomeTeamID = g.Item1,
                        AwayTeamID = g.Item2
                    });
                }
                db.SaveChanges();
                
                // Results Logic

                foreach (var g in db.Games)
                {

                    Console.WriteLine(
                        db.Teams.Where(t => t.TeamID == g.HomeTeamID).Select(t => t.Name).Single() + 
                        " vs " 
                        + db.Teams.Where(t => t.TeamID == g.AwayTeamID).Select(t => t.Name).Single());
                    Console.WriteLine("Did HomeTeam Win , Lose or was it Draw?");
                    
                    string result = Console.ReadLine();

                    if (result.Equals("Win", StringComparison.InvariantCultureIgnoreCase))
                    {
                        db.Teams.First(t => t.TeamID == g.HomeTeamID).Points += 3;
                        db.Teams.First(t => t.TeamID == g.AwayTeamID).Points -= 1;
                    }
                    else if (result.Equals("Lose", StringComparison.InvariantCultureIgnoreCase))
                    {
                        db.Teams.First(t => t.TeamID == g.HomeTeamID).Points -= 1;
                        db.Teams.First(t => t.TeamID == g.AwayTeamID).Points += 3;
                    }
                    else
                    { }
                }
                db.SaveChanges();
           }
        }
    }

    public class FootballDBContext : DbContext
    {
        public DbSet<Team> Teams { get; set; }
        public DbSet<Game> Games { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server=DESKTOP-51E8SUL\\SQLEXPRESS;database=Football;Integrated security=true; MultipleActiveResultSets = true;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Game>()
                .HasOne(m => m.HomeTeam)
                .WithMany(t => t.HomeGames)
                .HasForeignKey(m => m.HomeTeamID)
                .IsRequired(false);

            modelBuilder.Entity<Game>()
                .HasOne(m => m.AwayTeam)
                .WithMany(t => t.AwayGames)
                .HasForeignKey(m => m.AwayTeamID)
                .IsRequired(false);
        }
    }

    public class Team
    {   
        public int TeamID { get; set; }

        public int Points { get; set; }

        public string Name { get; set; }
        
        [InverseProperty("HomeTeam")]
        public virtual List<Game> HomeGames { get; set; }
        [InverseProperty("AwayTeam")]
        public virtual List<Game> AwayGames { get; set; }
    }

    public class Game
    {
        public int GameID { get; set; }

        public int HomeTeamID { get; set; }
        public int AwayTeamID { get; set; }
        
        public virtual Team HomeTeam { get; set; }
        public virtual Team AwayTeam { get; set; }
    }
}
