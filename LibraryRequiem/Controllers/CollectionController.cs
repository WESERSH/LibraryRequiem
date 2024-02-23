using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryRequiem.Data;
using LibraryRequiem.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using static System.Collections.Specialized.BitVector32;
using Microsoft.Identity.Client;
using System.Xml.XPath;
using System.Security.Cryptography.Pkcs;
using Microsoft.AspNetCore.Authorization;
using LibraryRequiem.Models.ViewModel;
using static System.Reflection.Metadata.BlobBuilder;

namespace LibraryRequiem.Controllers
{
    public class CollectionController : Controller
    {
        //Создание переменных для информации об окружении веб-приложения и контекста базы данных
        private readonly CollectionContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDbContextFactory<CollectionContext> _dbContextFactory;

        //Инициализация этих переменных
        public CollectionController(CollectionContext context, IWebHostEnvironment webHostEnvironment, IDbContextFactory<CollectionContext> dbContextFactory)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _dbContextFactory = dbContextFactory;
        }

        //Переопределение метода OnActionExecuting
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Помещение всех полей Genre из базы данных в переменную Genres
            IEnumerable<string> Genres = _context.Books.Select(g => g.Genre);

            //Исключение повторяющихся элементов
            Genres = Genres.Distinct();

            //Помещение Genres во ViewBag
            ViewBag.Genres = Genres;

            //Сохранение изменений
            base.OnActionExecuting(context);
        }

        // GET: Collection
        public async Task<IActionResult> Index()
        {
            var books = await _context.Books.ToListAsync();
            var viewModel = new CollectionIndexViewModel();

            if (User.Identity.IsAuthenticated)
            {
                using(var context = _dbContextFactory.CreateDbContext())
                {
                    UserModel user = new UserModel();

                    user = await context.Users.Include(x => x.FavoriteList.Books)
                .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

                    viewModel.User = user;
                    viewModel.Books = books;

                }

                return View(viewModel);
            }

            viewModel.User = null;
            viewModel.Books = books;
            //Возврат объектов бд в виде списка
            return _context.Books != null ? 
                          View(viewModel) :
                          Problem("Entity set 'CollectionContext.Books'  is null.");
        }

        //GET collection/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            //Проверка на null и возврат NotFound
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }
            try
            {
                //Проверка id и бд на null и возврат NotFound
                var bookModel = await _context.Books.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

                //Создание пути к загружакмому файлу
                var pathToDownload = Path.GetFullPath($@"{_webHostEnvironment.WebRootPath}{bookModel.BookFilePath}");

                //Преобразование файла в массив байтов
                byte[] fileBytes = System.IO.File.ReadAllBytes(pathToDownload);

                //Получение имени файла
                string fileName = pathToDownload;
                fileName = Path.GetFileName(fileName);

                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch { return NotFound(); }
        }

        //GET:
        public async Task<IActionResult> SearchResult(string? searchitem, string? searchGenre)
        {
            var books = _context.Books.AsNoTracking();
            var viewModel = new CollectionIndexViewModel();

            //Проверка поля поискового запроса на null
            if (searchitem != null)
            {
                //Фильтрация объектов бд и помещение в searchResult
                var searchResult = books.Where(p => EF.Functions.Like(p.Title, $"%{searchitem}%") || EF.Functions.Like(p.Author, $"%{searchitem}%")).ToListAsync();

                ViewBag.Search = $"Поиск по запросу: {searchitem}";

                ViewBag.SearchField = searchitem;

                if (User.Identity.IsAuthenticated)
                {
                    using (var context = _dbContextFactory.CreateDbContext())
                    {
                        UserModel user = new UserModel();

                        user = await context.Users.Include(x => x.FavoriteList.Books)
                    .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

                        viewModel.User = user;
                        viewModel.Books = await searchResult;

                    }

                    return View(viewModel);
                }

                viewModel.User = null;
                viewModel.Books = await searchResult;

                return View(viewModel);
            }

            //Проверка поля поискового запроса на null
            else if (searchGenre != null)
            {
                //Фильтрация объектов бд и помещение в searchResult
                var searchResult = books.Where(p => EF.Functions.Like(p.Genre, $"%{searchGenre}%")).ToListAsync();

                ViewBag.Search = $"Книги жанра: {searchGenre}";

                if (User.Identity.IsAuthenticated)
                {
                    using (var context = _dbContextFactory.CreateDbContext())
                    {
                        UserModel user = new UserModel();

                        user = await context.Users.Include(x => x.FavoriteList.Books)
                    .FirstOrDefaultAsync(x => x.UserName == User.Identity.Name);

                        viewModel.User = user;
                        viewModel.Books = await searchResult;

                    }

                    return View(viewModel);
                }

                viewModel.User = null;
                viewModel.Books = await searchResult;

                return View(viewModel);
            }

            return View();
        }

        // GET: Collection/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //Проверка id и бд на null и возврат NotFound
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            //Поиск объекта бд по его id
            var bookModel = await _context.Books.FindAsync(id);

            if (bookModel == null)
            {
                return NotFound();
            }

            return View(bookModel);
        }

        // GET: Collection/Create


        [Authorize]
        [AllowAnonymous]
        public IActionResult Create()
        {
            if (!User.IsInRole("admin"))
            {
                ModelState.AddModelError("", "Вы не админ!!!");
                return PartialView("../Accounts/login");
            };

            return View();
        }

        // POST: Collection/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [ValidateAntiForgeryToken]
        [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue)]
        [RequestSizeLimit(int.MaxValue)]
        [HttpPost]
        [Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> Create(IFormFile bookImage, IFormFile bookFile, [Bind("Id,Title,Author,Genre,Description,DateOfUpload,Tags")] BookModel bookModel)
        {
            // Создание путей к обложке книги и ее файлу
            bookModel.BookFilePath = @$"/Files/Book{bookModel.Id}/{bookFile.FileName}";
            bookModel.BookImagePath = @$"/Files/Book{bookModel.Id}/{bookImage.FileName}";

            // Добавление модели в базу данных
            _context.Add(bookModel);
            await _context.SaveChangesAsync();

            // Создание путей в файловой системе для обложки книги и ее файла
            var pathToSaveImage = Path.GetFullPath(@$".\wwwroot\Files\Book{bookModel.Id}\{bookImage.FileName}");
            var pathToSaveFile = Path.GetFullPath($@".\wwwroot\Files\Book{bookModel.Id}\{bookFile.FileName}");

            // Заполнение полей модели значениями
            bookModel.BookFilePath = @$"/Files/Book{bookModel.Id}/{bookFile.FileName}";
            bookModel.BookImagePath = @$"/Files/Book{bookModel.Id}/{bookImage.FileName}";
            bookModel.Id = bookModel.Id;
            bookModel.DateOfUpload = DateTime.Now;

            // Проверка формата файла книги
            if (Path.GetExtension(bookFile.FileName) != ".fb2" && Path.GetExtension(pathToSaveFile) != ".fb2")
            {
                ModelState.AddModelError("BookFilePath", "Неверный формат книги");
            }

            // Проверка формата обложки книги
            if (Path.GetExtension(bookImage.FileName) != ".jpg" && Path.GetExtension(pathToSaveImage) != ".png")
            {
                ModelState.AddModelError("BookImagePath", "Неверный формат изображения");
            }

            // Получение информации о директории
            DirectoryInfo directoryInfo = new DirectoryInfo($@"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\");

            // Проверка на правильность заполненных форм, сохранение изменений в базе данных и добавление файлов в файловую систему
            if (ModelState.IsValid)
            {
                directoryInfo.Create();

                using (var stream = new FileStream(pathToSaveFile, FileMode.Create))
                {
                    await bookFile.CopyToAsync(stream);
                }
                using (var stream = new FileStream(pathToSaveImage, FileMode.Create))
                {
                    await bookImage.CopyToAsync(stream);
                }

                _context.Entry(bookModel).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Удаление объекта из базы данных в случае неправильного его заполнения
            _context.Books.Remove(bookModel);
            await _context.SaveChangesAsync();
            return View(bookModel);
        }

        // GET: Collection/Edit/5

        [Authorize]
        [AllowAnonymous]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!User.IsInRole("admin"))
            {
                ModelState.AddModelError("", "Вы не админ!!!");
                return PartialView("../Accounts/login");
            };

            //Проверка id и бд на null и возврат NotFound
            if (id == null || _context.Books == null)
            {
                return NotFound();
            }

            //Поиск объекта бд по его id
            var bookModel = await _context.Books.FindAsync(id);

            /*var imagePath = @$"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{Path.GetFileName(bookModel.BookImagePath)}";

            var filePath = @$"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{Path.GetFileName(bookModel.BookFilePath)}";

            ViewBag.ImagePath = imagePath;

            ViewBag.FilePath = filePath;*/

            if (bookModel == null)
            {
                return NotFound();
            }
            return View(bookModel);
        }

        // POST: Collection/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(IFormFile? bookImage, IFormFile? bookFile, [Bind("Id,Title,Author,Genre,Description,DateOfUpload,Tags,BookFilePath,BookImagePath")] BookModel bookModel)
        {

            //Проверка на вальдность модели книги
            if (ModelState.IsValid)
            {
                try
                {
                    //Пути для удаления старых файлов в файловой системе
                    var bookFilePathToDelete = Path.GetFullPath(@$"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{Path.GetFileName(bookModel.BookFilePath)}");
                    var bookImagePathToDelete = Path.GetFullPath(@$"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{Path.GetFileName(bookModel.BookImagePath)}");

                    //Пути для сохранения новых файлов в файловой системе
                    var bookImagePathToSave = Path.GetFullPath(@$"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{bookImage.FileName}");
                    var bookFilePathToSave = Path.GetFullPath($@"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\{bookFile.FileName}");

                    //Проверка формата файла книги
                    /*if (Path.GetExtension(bookFilePathToSave) != ".fb2")
                    {
                        ModelState.AddModelError("BookFilePath", "Неверный формат книги");
                    }*/ 
                    //Проверка формата обложки книги
                    if (Path.GetExtension(bookImagePathToSave) != ".jpg" && Path.GetExtension(bookImagePathToSave) != ".png")
                    {
                        ModelState.AddModelError("BookImagePath", "Неверный формат изображения");
                    }

                    //Проверка на правильность заполненных форм, сохранение изменений в бд, удаление старых и запись новых файлов в файловую систему
                    if (ModelState.IsValid)
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo($@"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\");
                        FileInfo deleteImage = new FileInfo(bookImagePathToDelete);
                        FileInfo deleteFile = new FileInfo(bookFilePathToDelete);
                        deleteFile.Delete();
                        deleteImage.Delete();
                        directoryInfo.Create();

                        using (var stream = new FileStream(bookImagePathToSave, FileMode.Create))
                        {
                            await bookImage.CopyToAsync(stream);
                        }
                        using (var stream = new FileStream(bookFilePathToSave, FileMode.Create))
                        {
                            await bookFile.CopyToAsync(stream);
                        }

                        bookModel.BookImagePath = $"/Files/Book{bookModel.Id}/{bookImage.FileName}";
                        bookModel.BookFilePath = $"/Files/Book{bookModel.Id}/{bookFile.FileName}";

                        _context.Entry(bookModel).State = EntityState.Modified;
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }
                    return View(bookModel);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookModelExists(bookModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(bookModel);
        }

        // POST: Collection/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //Проверка на бд на null
            if (_context.Books == null)
            {
                return Problem("Entity set 'CollectionContext.Books'  is null.");
            }

            //Поиск объекта бд по id
            var bookModel = await _context.Books.FindAsync(id);

            if (bookModel != null)
            {
                //Получение пути для удаления директории с файлами книги
                var bookPathToDelete = Path.GetFullPath($@"{_webHostEnvironment.WebRootPath}\Files\Book{bookModel.Id}\");
                DirectoryInfo deleteFile = new DirectoryInfo(bookPathToDelete);
                try
                {
                    deleteFile.Delete(true);
                }
                catch {}

                //Удаление записи из бд
                _context.Books.Remove(bookModel);

                var favotitLists = _context.FavoriteLists.Include(x => x.Books);
                foreach( var favotitList in favotitLists)
                {
                    if (favotitList.Books != null)
                    {
                        var bookToDelete = favotitList.Books.FirstOrDefault(x => x.BookId == id);
                        favotitList.Books.Remove(bookToDelete);
                    }
                }
            }

            //Сохранение изменений
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReadPage(int id)
        {
            var bookModel = await _context.Books.FindAsync(id);

            if (bookModel == null) { return View(); }

            var path = _webHostEnvironment.WebRootPath + @$"/Files/Book{bookModel.Id}/" + Path.GetFileName(bookModel.BookFilePath);

            XmlDocument fileDoc = new XmlDocument();
            fileDoc.Load(path);

            XmlElement root = fileDoc.DocumentElement;

            List<string> paragraphs = new List<string>();
            string chapter = string.Empty;

            XmlNode description = null;
            XmlNode body = null;
            List<XmlNode> binary = new List<XmlNode>();
            List<XmlNode> sections = new List<XmlNode>();

            foreach (XmlNode node in root.ChildNodes)
            {
                switch (node.LocalName)
                {
                    case "description":
                        description = node;
                        break;
                    case "body":
                        body = node;
                        foreach (XmlNode section in node.ChildNodes)
                        {
                            sections.Add(section);
                        }
                        break;
                    case "binary":
                        binary.Add(node);
                        break;
                }
            }

           foreach (XmlNode node in sections)
            {
                foreach (XmlNode ChildSection in node.ChildNodes)
                {
                    switch (ChildSection.LocalName)
                    {
                        case "p":
                            foreach (XmlNode p in ChildSection.ChildNodes)
                            {
                                if (p.LocalName == "strong")
                                {
                                    if (p.LastChild.LocalName == "sup")
                                    {
                                        paragraphs.Add($"<strong> {p.FirstChild.InnerText} <{p.LastChild.LocalName}> {p.LastChild.InnerText} </{p.LastChild.LocalName}></strong>");
                                    }
                                    else
                                    {
                                        paragraphs.Add($"<h3><strong>{p.InnerText}</strong></h3>");
                                    }
                                }

                                else if (p.LocalName == "emphasis")
                                {
                                    paragraphs.Add($"<p class = \"emphasis\">{p.InnerText}</p>");
                                }

                                else if (p.LocalName == "image")
                                {
                                    XmlAttribute hrefAttribute = p.Attributes["l:href"];

                                    foreach (XmlNode binaryNode in binary)
                                    {
                                        XmlAttribute binaryAttribute = binaryNode.Attributes["id"];

                                        string valueHrefAttribute = hrefAttribute.Value;

                                        valueHrefAttribute = valueHrefAttribute.Replace("#", "");

                                        if (valueHrefAttribute == binaryAttribute.Value)
                                        {
                                            paragraphs.Add("<img src = \"data:image/jpeg;base64," + binaryNode.InnerText + "\"/>");
                                        }
                                    }
                                }
                                else if (p.LocalName == "#text" || p.LocalName == "a")
                                {
                                    if (p.NextSibling != null && p.PreviousSibling == null)
                                    {
                                        if (p.NextSibling.LocalName == "sup")
                                        {
                                            paragraphs.Add($"<p>{p.InnerText}<sup>{p.NextSibling.InnerText}</sup>");
                                        }

                                        else if (p.NextSibling.LocalName == "a")
                                        {
                                            paragraphs.Add($"<p>{p.InnerText} <{p.NextSibling.LocalName} {p.NextSibling.Attributes["l:href"].LocalName}={p.NextSibling.Attributes["l:href"].Value}> {p.NextSibling.InnerText} </{p.NextSibling.LocalName}>");
                                        }
                                        else
                                        {
                                            paragraphs.Add($"{p.ParentNode.InnerText}");
                                        }
                                    }
                                    else if (p.PreviousSibling != null && p.NextSibling != null)
                                    {
                                        if (p.NextSibling.LocalName == "sup")
                                        {
                                            paragraphs.Add($"{p.InnerText}<sup>{p.NextSibling.InnerText}</sup>");
                                        }

                                        else if ( p.PreviousSibling.LocalName == "a")
                                        {
                                            paragraphs.Add($"<{p.NextSibling.LocalName} {p.NextSibling.Attributes["l:href"].LocalName}={p.NextSibling.Attributes["l:href"].Value}> {p.NextSibling.InnerText} </{p.NextSibling.LocalName}>");
                                        }
                                        else
                                        {
                                            paragraphs.Add($"{p.ParentNode.InnerText}");
                                        }
                                    }
                                    else if (p.PreviousSibling != null && p.NextSibling == null)
                                    {
                                        if (p.PreviousSibling.LocalName == "sup")
                                        {
                                            paragraphs.Add($"{p.InnerText}</p>");
                                        }
                                        else if (p.PreviousSibling.LocalName == "a")
                                        {
                                            paragraphs.Add("</ p >");
                                        }
                                        else
                                        {
                                            paragraphs.Add($"<p>{p.ParentNode.InnerText}</p>");
                                        }
                                    }

                                    else if (p.LocalName == "a")
                                    {
                                        paragraphs.Add($"<p><a href = \"{p.Attributes["l:href"].Value}\">{p.InnerText}</a></p>");
                                    }
                                    else
                                    {
                                        paragraphs.Add ($"<p>{p.InnerText}</p>");
                                    }
                                }
                            }
                            break;
                        case "section":
                            foreach (XmlNode p in ChildSection.ChildNodes)
                            {
                                switch (p.LocalName)
                                {
                                    case "p":
                                        paragraphs.Add($"<p>{p.InnerText}</p>");
                                        break;

                                    case "title" or "subtitle":
                                        string tag = null;

                                        if (p.LocalName == "title") { tag = "strong"; }

                                        else if (p.LocalName == "subtitle") { tag = "p"; }

                                        paragraphs.Add($"<{tag}>{p.FirstChild.InnerText}</{tag}>");
                                        break;


                                    case "image":
                                        XmlAttribute hrefSectionInSectionAttribute = p.Attributes["l:href"];

                                        foreach (XmlNode binaryNode in binary)
                                        {
                                            XmlAttribute binaryAttribute = binaryNode.Attributes["id"];

                                            string valueHrefAttribute = hrefSectionInSectionAttribute.Value;

                                            valueHrefAttribute = valueHrefAttribute.Replace("#", "");

                                            if (valueHrefAttribute == binaryAttribute.Value)
                                            {
                                                paragraphs.Add("<img src = \"data:image/jpeg;base64," + binaryNode.InnerText + "\"/>");
                                            }
                                        }
                                        break;
                                    case "empty-line":

                                        paragraphs.Add("<br />");
                                        break;
                                }
                            }
                            break;
                        case "empty-line":

                            paragraphs.Add("<br />");
                            break;
                        case "title":
                            foreach (XmlNode p in ChildSection.ChildNodes)
                            {
                                paragraphs.Add($"<strong>{p.InnerText}</strong><br/>");
                            }
                            break;
                        case "subtitle":
                            paragraphs.Add($"<strong>{ChildSection.Value}</strong><br/>");
                            break;
                        case "image":
                            XmlAttribute hrefSectionAttribute = ChildSection.Attributes["l:href"];

                            foreach (XmlNode binaryNode in binary)
                            {
                                XmlAttribute binaryAttribute = binaryNode.Attributes["id"];

                                string valueHrefAttribute = hrefSectionAttribute.Value;

                                valueHrefAttribute = valueHrefAttribute.Replace("#", "");

                                if (valueHrefAttribute == binaryAttribute.Value)
                                {
                                    paragraphs.Add("<img src = \"data:image/jpeg;base64," + binaryNode.InnerText + "\"/>");
                                }
                            }
                            break;
                    }
                }
            }
            
            ViewBag.Book = paragraphs;

            return View();

        }

        private bool BookModelExists(int id)
        {
          return (_context.Books?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
