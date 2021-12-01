using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestSharp;
using RestSharp.Authenticators;
using country_challenge_rest.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Net.Mime;
using System.IO;
using System.Text;
using CsvHelper;
using System.Globalization;

namespace country_challenge_rest.Controllers
{
    public class InicioController : Controller
    {
        // GET: Inicio
        public ActionResult Index()
        {
          
            return View();
        }

        public List<Pais> Carregar_Paises()
        {
            List<Pais> paises = new List<Pais>();
            var client = new RestClient("https://restcountries.com/v3.1");
            var request = new RestRequest("all", DataFormat.Json);
            var response = client.Get(request);

            List<Pais> paises_ = JsonConvert.DeserializeObject<List<Pais>>(response.Content);

                foreach (var item in paises_)
                {
                    Pais p = new Pais()
                    {
                        Name=new Name() { Common=item.Name.Common,Official=item.Name.Official,NativeName=item.Name.NativeName},
                        Capital=item.Capital,
                        Region=item.Region,
                        Ccn3=item.Ccn3,
                    };
                    paises.Add(p);
                }
            
            return paises;


        }

        [HttpPost]
        public ActionResult carregarTabela(int draw, int start, int length)
        {
            // Procurar Registos 
            string search = Request.Form.GetValues("search[value]").FirstOrDefault();
            // Ordenar colunas
            var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
            var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
            if (Request.IsAjaxRequest())
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                int recordFilteredTotal = 0;
                // carregar Registos na api.   
                var paises = Carregar_Paises();
                // 1. procurar  
                if (!string.IsNullOrEmpty(search))
                {
                    paises = paises.Where(u => u.Name.Official.ToUpper().Contains(search.ToUpper())).ToList();
                }
                // 2. obter o numero total de registos 
                recordsTotal = paises.Count();
                // 3. ordenar  
                var filteredData = paises as IEnumerable<Pais>;
                Func<Pais, string> orderingFunction = (c => sortColumn == "Name.Official" ? c.Name.Official.ToString() :
                                          sortColumn == "Capital" ? c.Capital[0] :
                                          sortColumn == "Ccn3" ? c.Ccn3 :
                                           sortColumn == "Region" ? c.Region
                                          : c.Name.Official);
                if (sortColumnDir == "asc")
                    filteredData = filteredData.OrderBy(orderingFunction);
                else
                    filteredData = filteredData.OrderByDescending(orderingFunction);
                // 4. filtrar  
                filteredData = filteredData.Skip(skip).Take(pageSize).ToList();
                // 5. obter o numero de registors filtrados 
                recordFilteredTotal = filteredData.Count();
                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordFilteredTotal, data = filteredData }, JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        public ActionResult Detalhes_pais(string Ccn3)
        {
            try
            {
                var client = new RestClient("https://restcountries.com/v3.1");
                var request = new RestRequest("alpha/" + Ccn3, DataFormat.Json);
                var response = client.Get(request);

                List<Pais> pais = JsonConvert.DeserializeObject<List<Pais>>(response.Content);
                ViewBag.pais = pais.First();
            }
            catch (Exception)
            {

                return View("Error");
            }
            
            
            return View();
        }

        public ActionResult export_xml()
        {
            try
            {
                List<Pais> paises = new List<Pais>();
                var client = new RestClient("https://restcountries.com/v3.1");
                var request = new RestRequest("all", DataFormat.Json);
                var response = client.Get(request);

                List<Pais> paises_ = JsonConvert.DeserializeObject<List<Pais>>(response.Content);
                XmlDocument doc = JsonConvert.DeserializeXmlNode("{'Row':" + response.Content + "}", "root");
                byte[] bytes = Encoding.Default.GetBytes(doc.OuterXml);
                var fileStreamResult = File(bytes, "application/octet-stream", "Paises.xml");
                return fileStreamResult;
            }
            catch (Exception)
            {

                return View("Error");
            }
            
        }

      
    }
}