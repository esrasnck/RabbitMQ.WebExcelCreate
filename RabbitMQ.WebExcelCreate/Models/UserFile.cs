using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.WebExcelCreate.Models
{
    public enum FileStatus
    {
        Creating, Completed // kullanıcı butona bastığında satır oluştururken daha olumadığı için creatingte tutacam. Ama excell oluşunca completed a çekecem
    }
    public class UserFile
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string FileName { get; set; } // excell dosyası ismi

        public string FilePath { get; set; }  // excell dosyasını nereden indirecek

        public DateTime? CreatedDate { get; set; } // bastığında oluşturmuyorum. ne zaman worker service bittiğinde o zaman dosya oluşturma tarihi yazılacak

        public FileStatus FileStatus { get; set; }

        // bu tarih yoksa bana dümdüz bir tarih dönsün. veritabanına mapplenmesin diye notmapped diyorum

        [NotMapped]
        public string GetCreatedDate => CreatedDate.HasValue ? CreatedDate.Value.ToShortDateString() : "-";  // get metodunu yazıyoz. seti yok



    }


}
