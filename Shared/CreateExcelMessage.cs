using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class CreateExcelMessage
    {
        public string UserId { get; set; }
        public int FileId { get; set; }


        // oluşturulacak excell dosyasını da göndermek istersek, byte array olarak gönderebiliriz. bu tamamen tercih meselesi. excelin dosya boyutu çok büyükse, çok fazla tablodan oluşan bir yapı ise, bunu göndermek çok mantıklı değil. onun yerine broker service'in direkt veritabanına bağlanıp, oradan çekmesi daha mantıklı. Ama daha küçük sayılar varsa, direkt olarak excel oluşacak dataları buradan gönderebiliriz. zorda kalınırsa yapılabilir. mesaja gömülebilir. ama büyük dataları db den çekmesi daha mantıklı. biz bu senaryoda bunu yapacaz. bu yüzden burada üçüncü bir property belirtmiyoruz. ama kullansaydık, belirtirdik.
    }
}
