namespace Europe.CentralBank.CashServer.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.cash",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        amount = c.Int(nullable: false),
                        created_at = c.DateTimeOffset(nullable: false, precision: 7),
                        digital = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.id);
            
            CreateTable(
                "dbo.cash_invalidation",
                c => new
                    {
                        cash_id = c.Int(nullable: false),
                        invalidated_cash_id = c.Int(nullable: false),
                        cash_id1 = c.Int(),
                        invalidated_cash_id1 = c.Int(),
                    })
                .PrimaryKey(t => new { t.cash_id, t.invalidated_cash_id })
                .ForeignKey("dbo.cash", t => t.cash_id1)
                .ForeignKey("dbo.cash", t => t.invalidated_cash_id1)
                .ForeignKey("dbo.cash", t => t.invalidated_cash_id)
                .ForeignKey("dbo.cash", t => t.cash_id)
                .Index(t => t.cash_id)
                .Index(t => t.invalidated_cash_id)
                .Index(t => t.cash_id1)
                .Index(t => t.invalidated_cash_id1);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.cash_invalidation", "cash_id", "dbo.cash");
            DropForeignKey("dbo.cash_invalidation", "invalidated_cash_id", "dbo.cash");
            DropForeignKey("dbo.cash_invalidation", "invalidated_cash_id1", "dbo.cash");
            DropForeignKey("dbo.cash_invalidation", "cash_id1", "dbo.cash");
            DropIndex("dbo.cash_invalidation", new[] { "invalidated_cash_id1" });
            DropIndex("dbo.cash_invalidation", new[] { "cash_id1" });
            DropIndex("dbo.cash_invalidation", new[] { "invalidated_cash_id" });
            DropIndex("dbo.cash_invalidation", new[] { "cash_id" });
            DropTable("dbo.cash_invalidation");
            DropTable("dbo.cash");
        }
    }
}
