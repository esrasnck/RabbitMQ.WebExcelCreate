using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.WebExcelCreate.Models;
using RabbitMQ.WebExcelCreate.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;
        private readonly RabbitMQPublisher _rabbitMQPublisher;

        public ProductController(UserManager<IdentityUser> userManager, AppDbContext context, RabbitMQPublisher rabbitMQPublisher)
        {
            _context = context;
            _userManager = userManager;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public IActionResult Index()
        {
            return View();
        }

        // create product excel // file metodu olacak file kısmının frontendi olacak ki kullanıcı daha önce oluşturduğu dosyaları da indirebilsin.
        // create product excel ise, veritabanında dosyayı tutacak. rabbit e göndercek. oluşturma tamamlandığı zaman da ilgili satırı bulup, update edecek


        public async Task<IActionResult> CreateProductExcel()
        {
            // hangi tablonun excel oluşturcağını bildiğimden parametre vermiyorum.
            // user için usermanager a ihtiyacım var. bir de veritabanına kaydetmek için metot gelecek. bir de rabbit'e mesaj göndercez

            var user =await  _userManager.FindByNameAsync(User.Identity.Name);  // cookie de bu bulunuyor. User'ın propertsi contorllerden geliyor? miras aldığımız?

            var fileName = $"product-excel-{Guid.NewGuid().ToString().Substring(1, 10)}";

            UserFile userFile = new()
            {
                UserId = user.Id,
                FileName = fileName,
                FileStatus = FileStatus.Creating
            };

            await _context.UserFiles.AddAsync(userFile);

            await _context.SaveChangesAsync();

            // bir requestten diğer bir requeste data taşıyorum. viewbag=> aynı requestte datayı model tarafına taşıyabiliriz. ama bir requestten diğer requestte data taşımak için temData kullanılmalı. bu de cookie de bunu saklar (session diye hatırlıyordum ben :( } okunduğu anda da cookie yi siliyor.(temp datayı)


            // veritabanındaki id yi efcore gelip otomatik olarak dolduruyor.
            _rabbitMQPublisher.Publish(new Shared.CreateExcelMessage() { FileId = userFile.Id});

            TempData["StartCreatingExcel"] = true;



            return RedirectToAction(nameof(Files));

        }

        public async Task<IActionResult> Files()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);


            return View( await _context.UserFiles.Where(x=> x.UserId == user.Id).ToListAsync());
        }

    }
}
