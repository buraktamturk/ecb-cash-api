using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Europe.CentralBank.CashServer.Models;
using Europe.CentralBank.CashServer.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Europe.CentralBank.CashServer.Controllers {
    public class Cash {
        public int? id { get; set; }

        public int? amount { get; set; }

        public bool? digital { get; set; }

        public string data { get; set; }

        public DateTimeOffset? created_at { get; set; }

        public DateTimeOffset? invalidated_at { get; set; }
    }

    public class CashRequest {
        public List<string> cashs { get; set; }

        public List<int> amounts { get; set; }
    }

    public class CashController : Controller {
        private ApplicationDbContext db;
        private CashValidator cashValidator;

        public CashController(ApplicationDbContext db, CashValidator cashValidator) {
            this.db = db;
            this.cashValidator = cashValidator;
        }

        public static Expression<Func<cash, Cash>> cash = a => new Cash {
            id = a.id,
            amount = a.amount,
            created_at = a.created_at,
            invalidated_at = a.invalidated_by.DefaultIfEmpty().Min(b => b.cash.created_at)
        };

        [HttpGet("cash/{id}")]
        public async Task<Cash> _cash(int id) {
            return await db.cashs
                .Where(a => a.id == id)
                .Select(cash)
                .SingleAsync();
        }

        [HttpPost("cash")]
        public async Task<List<string>> invalidat_cash([FromBody] CashRequest cashRequest) {
            List<cash> cashs = new List<cash>();
            List<int> cash_ids = new List<int>();
            List<cash> out_cash = new List<cash>();
            
            foreach (var cashStr in cashRequest.cashs) {
                var cash = cashValidator.CashFromString(cashStr);
                cash_ids.Add(cash.id.Value);
            }

            cashs = await db.cashs
                .Where(a => cash_ids.Contains(a.id))
                .ToListAsync();

            if (cashs.Count != cashRequest.cashs.Count) {
                throw new Exception("money count does not match");
            }

            if (cashRequest.amounts
                    .Sum() != cashs.Sum(b => b.amount)) {
                throw new Exception("input money must have same amount as output money.");
            }

            foreach(var amount in cashRequest.amounts) {
                var cash = new cash {
                    amount = amount,
                    created_at = DateTimeOffset.UtcNow,
                    digital = true
                };

                out_cash.Add(cash);
                db.cashs.Add(cash);
            }

            await db.SaveChangesAsync();

            return out_cash
                .Select(CashController.cash.Compile())
                .Select(a => cashValidator.CashToString(a))
                .ToList();
        }
    }
}
