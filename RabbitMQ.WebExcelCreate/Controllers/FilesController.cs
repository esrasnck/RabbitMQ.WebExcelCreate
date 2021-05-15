using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.WebExcelCreate.Hubs;
using RabbitMQ.WebExcelCreate.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        // formFile=> excell dosyası, userId=> hangi kullanıcı için(dosya id sinden useId bulunduğu için kullanmadık), fileId=> hangi file için. => bunları hep worker serviceden alacaz

        private readonly AppDbContext _context;

        private readonly IHubContext<MyHub> _hubContext;

        public FilesController(AppDbContext context, IHubContext<MyHub> hubContext)
        {
            _context = context;  // userFile için çağırıyorz
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, int fileId)
        {
            if (file is not { Length: > 0 }) return BadRequest();

            var userFile = await _context.UserFiles.FirstAsync(x => x.Id == fileId);

            // filename  ve dosyanın uzantısı nı ekliyoruz. yani file'ın name'in uzantısı .xlx gibi. bunu bir filePath'e atıyoruz.
            // amaç uzantısıyla beraber dosyanın ismini yazmak.
            var filePath = userFile.FileName + Path.GetExtension(file.FileName);

            //wwwroot files a kaydedeceğim path i alıyorum
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files", filePath);

            using FileStream stream = new(path, FileMode.Create);

            // file'ı stream e kopyalıyorum. dosya stream e kaydoldu

            await file.CopyToAsync(stream);

            // excel dosyası geldiğine göre bunu set edebiliriz
            userFile.CreatedDate = DateTime.Now;

            //path'ini veriyoruz.
            userFile.FilePath = filePath;

            // statusunu güncelliyoruz. file oluştu
            userFile.FileStatus = FileStatus.Completed;
            await _context.SaveChangesAsync();

            // signalR notification oluşturulacak.

            // bu bilgiyi hangi kullanıcı oluşturduysa ona göndereceğiz
            await _hubContext.Clients.User(userFile.UserId).SendAsync("CompletedFile");  // userfile dan gelen userId yi yakaladık (client o) mesajı gönderceğiz
            // bu metoda subcribe olduğunda clientlar işlem tamamlanacak. dinleme işlemi de layout da gerçekleşecek. sürekli dinleyeceğiz. kullanıcı sayfalarda dolansa da dinlesin. o yüzden layout da olmalı

            // bunu layoutta dinlemek için, SignalR javascript kütüphanesine ihtiyac var.

            return Ok();

         
        }


    }
}
