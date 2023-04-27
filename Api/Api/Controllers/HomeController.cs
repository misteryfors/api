using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Data;
using MySqlConnector;
using Newtonsoft.Json;

namespace Api.Controllers
{
    public class ServiceData
    {
        public string Name { get; set; }
        public int Price { get; set; }
    }
    public class ReportData
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public string ServiceName { get; set; }
        public double AvgResult { get; set; }
    }
    public class BloodData
    {
        public DataTable Table { get; set; }
        public DataTable SelectedBlood { get; set; }
        public DataTable ServicesTable { get; set; }
        public List<int> SelectedServices { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly string _connectionString;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _connectionString = "Server=127.0.0.1;Port=3306;Database=sys;Uid=root;Pwd=admin123;";
        }

        public IActionResult auth(string username, string password)
        {
            string query = "SELECT * FROM users WHERE login=@username AND password=@password;";

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);

                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        // Если найден пользователь с указанными логином и паролем,
                        // то сохраняем его имя в объекте Session
                        reader.Read();
                        // Получаем значение столбца "name" для найденного пользователя
                        string name = reader.GetString("name");
                        string type = Convert.ToString(reader.GetInt32("type"));

                        // Сохраняем имя пользователя в объекте Session
                        var now = DateTime.UtcNow;
                        HttpContext.Session.SetString("lastActive", now.ToString());
                        HttpContext.Session.SetString("name", name);
                        HttpContext.Session.SetString("type", type);
                        ViewData["name"] = name;
                        ViewData["type"] = type;

                        // Возвращаем данные пользователя
                        ServiceData data = new ServiceData { Name = "Authorized", Price = 0 };
                        return Json(new { name, type });
                    }
                    else
                    {
                        ServiceData data = new ServiceData { Name = "Incorrect username or password", Price = 0 };
                        return Ok(data);
                    }
                }
            }
        }
        [HttpPost]
        public IActionResult ClearSessionData()
        {
            HttpContext.Session.Remove("name");
            HttpContext.Session.Remove("type");

            return Ok();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult login()
        {
            return View();
        }

        public IActionResult log(string service_name, int service_price)
        {
            // получаем значение имени пользователя из сессии
            var userName = HttpContext.Session.GetString("UserName");

            // обработка полученных данных
            ServiceData data = new ServiceData { Name = service_name, Price = service_price };

            // добавляем имя пользователя в данные, которые будут переданы на страницу
            ViewData["UserName"] = userName;

            // возвращаем данные в виде объекта JSON
            return Ok(data);
        }

        public IActionResult an()
        {
            //if (HttpContext.Session.GetString("type") == "1")
            {
                return View();
            }
            //else
            //  return RedirectToAction("AccessDenied", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View("accesDenied");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult crot()
        {
            return Content("сам ты крот");
        }

        public IActionResult otchet()
        {

            return View();

        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult GetData(DateTime startDate, DateTime endDate)
        {
            string query = "SELECT * FROM blood WHERE date BETWEEN @startDate AND @endDate";
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        BloodData bloodData = new BloodData { Table = dataTable };
                        return View("BloodData", bloodData);
                    }
                }
            }
            return View("NoData");
        }



        public IActionResult BloodData(DateTime? startDate, DateTime? endDate)
        {

            long startTicks = startDate.HasValue ? new DateTimeOffset(startDate.Value).ToUnixTimeSeconds() * 1000 : DateTime.MinValue.Ticks;
            long endTicks = endDate.HasValue ? new DateTimeOffset(endDate.Value).ToUnixTimeSeconds() * 1000 : DateTime.MaxValue.Ticks;

            string query = "SELECT * FROM blood WHERE date BETWEEN @startTicks AND @endTicks";

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@startTicks", startTicks);
                command.Parameters.AddWithValue("@endTicks", endTicks);

                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                    else
                    {
                        DataTable dataTable = new DataTable();
                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                }
            }
        }
        public IActionResult serviceData(DateTime? startDate, DateTime? endDate)
        {

          
            string query = "SELECT * FROM services";

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);


                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                    else
                    {
                        DataTable dataTable = new DataTable();
                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                }
            }
        }
        public IActionResult patientData(DateTime? startDate, DateTime? endDate)
        {

        
            string query = "SELECT * FROM patients";

            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
               
                connection.Open();
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                    else
                    {
                        DataTable dataTable = new DataTable();
                        ViewBag.StartDate = startDate;
                        ViewBag.EndDate = endDate;
                        ViewBag.Table = dataTable;

                        return View();
                    }
                }
            }
        }
        public IActionResult Get()
        {
            DateTime startDate = new DateTime(2022, 4, 1);
            DateTime endDate = new DateTime(2022, 4, 30);

            BloodData bloodData = new BloodData();

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("https://localhost:44368/");
            HttpResponseMessage response = client.GetAsync($"api/data/getdata?startDate={startDate}&endDate={endDate}").Result;

            if (response.IsSuccessStatusCode)
            {
                string json = response.Content.ReadAsStringAsync().Result;
                bloodData = JsonConvert.DeserializeObject<BloodData>(json);
            }

            return View(bloodData);
        }
        [HttpPost]
        public IActionResult DeleteData(int id)
        {
            string query = "DELETE FROM blood WHERE id=@id";
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("BloodData");
        }
        [HttpPost]
        public IActionResult DeleteService(int id)
        {
            string query = "DELETE FROM services WHERE Code=@id";
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("serviceData");
        }
        public IActionResult DeletePatient(int id)
        {
            string query = "DELETE FROM patients WHERE id=@id";
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                command.ExecuteNonQuery();
            }
            return RedirectToAction("patientData");
        }


        public DataTable GetDataById(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var command = new MySqlCommand("SELECT * FROM blood WHERE id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = command.ExecuteReader();
            var dataTable = new DataTable();
            dataTable.Load(reader);

            return dataTable;
        }

        public IActionResult Edit(int id)
        {
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                BloodData model = new BloodData();
                model.SelectedServices = new List<int>();
                connection.Open();
                using (var command = new MySqlCommand("SELECT DISTINCT * FROM blood_services INNER JOIN blood ON blood_services.blood = blood.id WHERE blood.id = @id", connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        var table = new DataTable();
                        if (reader.HasRows)
                        { table.Load(reader); }
                                else
                        {
                            var connection2 = new MySqlConnection(_connectionString);
                            connection2.Open();
                            var command2 = new MySqlCommand("SELECT * FROM blood LEFT JOIN blood_services ON blood.id = blood_services.blood WHERE blood.id = @id2", connection2);
                            command2.Parameters.AddWithValue("@id2", id);
                            MySqlDataReader reader2 = command2.ExecuteReader();
                           
                            table.Load(reader2);
                            DataRow row = table.Select().FirstOrDefault();
                            if (row != null)
                            {
                                row["blood"] = id;
                            }
                        }
                        var connection1 = new MySqlConnection(_connectionString);
                        connection1.Open();
                        var command1 = new MySqlCommand("SELECT * FROM services", connection1);
                        MySqlDataReader reader1 = command1.ExecuteReader();
                        var table1 = new DataTable();
                        table1.Load(reader1);
                        ViewBag.ServicesTable = table1;
                        ViewBag.SelectedBlood = table;
                        var bloodData = new BloodData { Table = table };
                        return View(bloodData);
                    }
                }
            }
        }
        public void UpdateServices(int id, List<string> services)
        {
            Console.WriteLine(id);
            Console.WriteLine(services.Count);
            for (int i = 0; i < services.Count; i++) 
            Console.WriteLine(services[i]);
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                // Удаляем все записи в таблице blood_service для данного id
                var deleteCommand = new MySqlCommand("DELETE FROM `blood_services` WHERE `blood`=@id", conn);
                deleteCommand.Parameters.AddWithValue("@id", id);
                deleteCommand.ExecuteNonQuery();

                // Вставляем новые записи в таблицу blood_service
                foreach (var service in services)
                {
                    var insertCommand = new MySqlCommand("INSERT INTO `blood_services` (`blood`, `service`) VALUES (@id_blood, @id_service)", conn);
                    insertCommand.Parameters.AddWithValue("@id_blood", id);
                    insertCommand.Parameters.AddWithValue("@id_service", service);
                    
                    Console.WriteLine(insertCommand.ExecuteNonQuery());
                }
            }
        }
        [HttpGet]
        public IActionResult GetFilteredData(DateTime startDate, DateTime endDate)
        {
            return GetData(startDate, endDate);
        }
        
        public void AddOrder(int patientId, int serviceId, string date)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();

                var command = new MySqlCommand("INSERT INTO `orders` (`patient_id`, `service_id`, `date`) VALUES (@patientId, @serviceId, @date)", conn);
                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@serviceId", serviceId);
                command.Parameters.AddWithValue("@date", DateTimeOffset.Parse(date).ToUnixTimeMilliseconds());

                command.ExecuteNonQuery();
            }
        }
        
        public IActionResult AddData(string patient, string barcode, DateTime date)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // создаем SQL-запрос для получения максимального значения id
            var maxIdQuery = "SELECT MAX(id) FROM blood";
            using var getMaxIdCmd = new MySqlCommand(maxIdQuery, conn);
            int maxId = Convert.ToInt32(getMaxIdCmd.ExecuteScalar());

            // создаем SQL-запрос для добавления записи в таблицу
            var sql = "INSERT INTO blood (id, patient, barcode, date) VALUES (@id, @patient, @barcode, @date)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", maxId + 1);
            cmd.Parameters.AddWithValue("@patient", patient);
            cmd.Parameters.AddWithValue("@barcode", barcode);
            cmd.Parameters.AddWithValue("@date", new DateTimeOffset(date).ToUnixTimeSeconds() * 1000);

            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }
        public IActionResult AddService(string service, string Price)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // создаем SQL-запрос для получения максимального значения id
            var maxIdQuery = "SELECT MAX(Code) FROM Services";
            using var getMaxIdCmd = new MySqlCommand(maxIdQuery, conn);
            int maxId = Convert.ToInt32(getMaxIdCmd.ExecuteScalar());

            // создаем SQL-запрос для добавления записи в таблицу
            var sql = "INSERT INTO services (Code, Service, Price) VALUES (@id, @service, @Price)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", maxId + 1);
            cmd.Parameters.AddWithValue("@service", service);
            cmd.Parameters.AddWithValue("@Price", Price);
            
            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }

        public IActionResult AddPatient(string fullname, string email, string social_type, string passport_s, string passport_n)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            // создаем SQL-запрос для получения максимального значения id
            var maxIdQuery = "SELECT MAX(id) FROM patients";
            using var getMaxIdCmd = new MySqlCommand(maxIdQuery, conn);
            int maxId = Convert.ToInt32(getMaxIdCmd.ExecuteScalar());

            // создаем SQL-запрос для добавления записи в таблицу
            var sql = "INSERT INTO patients (id, fullname, email, social_type, passport_s, passport_n) VALUES (@id, @fullname, @email, @social_type, @passport_s, @passport_n)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", maxId + 1);
            cmd.Parameters.AddWithValue("@fullname", fullname);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@social_type", social_type);
            cmd.Parameters.AddWithValue("@passport_s", passport_s);
            cmd.Parameters.AddWithValue("@passport_n", passport_n);

            var rowsAffected = cmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                return RedirectToAction("Index");
            }

            return View("Error");
        }
        [HttpGet]
        public IActionResult Get(DateTime startDate, DateTime endDate, string reportType)
        {
            List<ReportData> data = new List<ReportData>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                string query = "";
                switch (reportType)
                {
                    case "services":
                        query = "SELECT COUNT(*) as Count, Date FROM Services WHERE Date >= @startDate AND Date <= @endDate GROUP BY Date;";
                        break;
                    case "services-list":
                        query = "SELECT * FROM Services WHERE Date >= @startDate AND Date <= @endDate;";
                        break;
                    case "patients":
                        query = "SELECT COUNT(DISTINCT PatientID) as Count, Date FROM Services WHERE Date >= @startDate AND Date <= @endDate GROUP BY Date;";
                        break;
                    case "patients-per-service":
                        query = "SELECT COUNT(DISTINCT PatientID) as Count, ServiceName, Date FROM Services WHERE Date >= @startDate AND Date <= @endDate GROUP BY ServiceName, Date;";
                        break;
                    case "average-results":
                        query = "SELECT AVG(Result) as AvgResult, Date FROM Services WHERE Date >= @startDate AND Date <= @endDate GROUP BY Date;";
                        break;
                    default:
                        return BadRequest("Invalid report type");
                }

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@startDate", startDate.Date);
                    command.Parameters.AddWithValue("@endDate", endDate.Date.AddDays(1).AddTicks(-1));

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ReportData report = new ReportData();
                            report.Date = reader.GetDateTime("Date");
                            report.Count = reader.GetInt32("Count");
                            report.ServiceName = reader.GetString("ServiceName");
                            report.AvgResult = reader.GetDouble("AvgResult");
                            data.Add(report);
                        }
                    }
                }
            }

            return Ok(data);
        }


    }
    }