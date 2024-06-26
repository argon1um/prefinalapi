﻿using AHRestAPI.Mappers;
using AHRestAPI.Models;
using AHRestAPI.ModelsDTO;
using AnimalHouseRestAPI.DataBase;
using AnimalHouseRestAPI.ModelsDTO;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using Xceed.Words.NET;

namespace AHRestAPI.Controllers
{
    [Route("/orders")]
    [ApiController]
    public class Orders : ControllerBase
    {
        JsonSerializerSettings mainSettings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        [HttpGet]
        [Route("/orders/{orderid}")]
        public ActionResult<OrderGetDTO> GetOrderOnId(int orderid)
        {
            List<OrderGetDTO> getDTO = new List<OrderGetDTO>();
            List<Order> orders = DataBaseConnection.Context.Orders.ToList().Where(x => x.OrderId == orderid).ToList();
            if (orders != null)
            {
                getDTO = OrdergetMapper.ConvertToGet(orders);
                return Content(JsonConvert.SerializeObject(getDTO, mainSettings));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpGet]
        [Route("/orders/orderslist/{clientphone}")]
        public ActionResult<List<OrderGetDTO>> GetOrdersList(long clientphone)
        {
            List<OrderGetDTO> getDTO = new List<OrderGetDTO>();
            List<Order> orders = DataBaseConnection.Context.Orders.ToList().Where(x => x.ClientPhone == clientphone).ToList();
            if (orders != null)
            {
                getDTO = OrdergetMapper.ConvertToGet(orders);
                return Content(JsonConvert.SerializeObject(getDTO, mainSettings));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("/orders/orderslist/")]
        public ActionResult<List<OrderGetDTO>> GetOrdersList()
        {
            List<OrderGetDTO> getDTO = new List<OrderGetDTO>();
            List<Order> orders = DataBaseConnection.Context.Orders.ToList();
            if (orders != null)
            {
                getDTO = OrdergetMapper.ConvertToGet(orders);
                return Content(JsonConvert.SerializeObject(getDTO, mainSettings));
            }
            else
            {
                return BadRequest();
            }

        }


        [HttpGet]
        [Route("/orders/payedorders/")]
        public ActionResult<List<OrderGetDTO>> GetPayedOrders()
        {
            List<OrderGetDTO> getDTO = new List<OrderGetDTO>();
            List<Order> orders = DataBaseConnection.Context.Orders.ToList().Where(x => x.OrderStatusid == 2).ToList();
            if (orders != null)
            {
                getDTO = OrdergetMapper.ConvertToGet(orders);
                return Content(JsonConvert.SerializeObject(getDTO, mainSettings));
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("/orders/unpayedorders/")]
        public ActionResult<List<OrderGetDTO>> GetunPayedOrders()
        {
            List<OrderGetDTO> getDTO = new List<OrderGetDTO>();
            List<Order> orders = DataBaseConnection.Context.Orders.ToList().Where(x => x.OrderStatusid == 1).ToList();
            if (orders != null)
            {
                getDTO = OrdergetMapper.ConvertToGet(orders);
                return Content(JsonConvert.SerializeObject(getDTO, mainSettings));
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("/orders/statuschange")]
        public ActionResult<Order> OrderStatusChange([FromQuery] int orderid)
        {
            Order order = DataBaseConnection.Context.Orders.ToList().FirstOrDefault(x => x.OrderId == orderid);
            if (order != null)
            {
                order.OrderStatusid = 3;
                DataBaseConnection.Context.SaveChanges();
                return Content("Статус изменён");
            }
            else
            {
                return BadRequest();
            }
        }


        [HttpPost]
        [Route("/orders/addneworder")]
        public ActionResult<OrderDTO> AddNewOrder([FromBody] OrderAddDTO orderdto)
        {
            OrderDTO orderDTO = new OrderDTO();
            int animalbreedid = 0;

            IActionResult AnimalCheck(decimal clientphone)
            {
                List<Animal> animals = DataBaseConnection.Context.Animals.ToList().Where(x => x.AnimalClientphone == clientphone).ToList();
                if (animals != null)
                {
                    foreach (Animal animal in animals)
                    {
                        if (animal.AnimalName.ToLower() == orderdto.animalName.ToLower())
                        {
                            orderDTO.AnimalId = animal.AnimalId;
                            return Ok();
                        }
                    }

                    List<Animalbreed> breeds = DataBaseConnection.Context.Animalbreeds.ToList();
                    foreach (Animalbreed breed in breeds)
                    {
                        if (orderdto.animalBreed.ToLower() == breed.AnimalbreedName.ToLower())
                        {
                            animalbreedid = breed.AnimalbreedId;
                            return Ok(animalbreedid);
                        }
                        else
                        {
                            Animalbreed newbreed = new()
                            {
                                AnimalbreedId = DataBaseConnection.Context.Animalbreeds.Count() + 1,
                                AnimalbreedName = breed.AnimalbreedName,
                                AnimalTypeid = DataBaseConnection.Context.Animaltypes.ToList().FirstOrDefault(x => x.AnimaltypeName == orderdto.animalType).AnimaltypeId,
                            };
                            DataBaseConnection.Context.Animalbreeds.Add(newbreed);
                            DataBaseConnection.Context.SaveChanges();
                            Animal animalnotbreed = new()
                            {
                                AnimalId = DataBaseConnection.Context.Animals.Count() + 1,
                                AnimalName = orderdto.animalName,
                                AnimalGen = orderdto.animalGen,
                                AnimalClientphone = orderdto.clientPhone,
                                AnimalBreedid = newbreed.AnimalbreedId,
                                AnimalHeight = (double)orderdto.animalHeight,
                                AnimalWeight = (double)orderdto.animalWeight,
                                AnimalOld = orderdto.animalAge,
                            };
                            orderDTO.AnimalId = animalnotbreed.AnimalId;
                            DataBaseConnection.Context.Animals.Add(animalnotbreed);
                            DataBaseConnection.Context.SaveChanges();
                            return Ok();
                        }
                    }

                    Animal animal1 = new()
                    {
                        AnimalId = DataBaseConnection.Context.Animals.Count() + 1,
                        AnimalName = orderdto.animalName,
                        AnimalGen = orderdto.animalGen,
                        AnimalClientphone = orderdto.clientPhone,
                        AnimalBreedid = animalbreedid,
                        AnimalHeight = (double)orderdto.animalHeight,
                        AnimalWeight = (double)orderdto.animalWeight,
                        AnimalOld = orderdto.animalAge
                    };
                    DataBaseConnection.Context.Animals.Add(animal1);
                    DataBaseConnection.Context.SaveChanges();
                    return Ok();
                }

                return BadRequest();
            }

            Client client = DataBaseConnection.Context.Clients.ToList().FirstOrDefault(x => x.ClientPhone == orderdto.clientPhone && x.ClientPassword != null);
            if (orderdto.admDate > orderdto.issueDate)
            {
                return BadRequest("wrong dates");
            }

            else
            {
                orderDTO.OrderNoteId = DataBaseConnection.Context.Orders.Max(x => x.OrderNoteid) + 1;
                if (client != null)
                {
                    orderDTO.ClientPhone = orderdto.clientPhone;
                    orderDTO.OrderId = DataBaseConnection.Context.Orders.Max(x => x.OrderId) + 1;
                    AnimalCheck(client.ClientPhone);
                    orderDTO.OrderStatusId = 1;
                    orderDTO.Totalprice = orderdto.Totalprice;
                    client.ClientCountoforders += 1;
                    orderDTO.AdmissionDate = orderdto.admDate;
                    orderDTO.IssueDate = orderdto.issueDate;
                    orderDTO.RoomId = DataBaseConnection.Context.Rooms.ToList().FirstOrDefault(x => x.RoomNumber == orderdto.roomId).RoomId;
                    Room room = DataBaseConnection.Context.Rooms.FirstOrDefault(x => x.RoomId == orderDTO.RoomId);
                    room.RoomStatusid = 2;
                    DataBaseConnection.Context.Clients.Update(client);
                    DataBaseConnection.Context.Orders.Add(OrdergetMapper.ConvertToOrder(orderDTO));
                    DataBaseConnection.Context.SaveChanges();
                    return Ok();
                }
                else
                {
                    Client client1 = new()
                    {
                        ClientId = DataBaseConnection.Context.Clients.ToList().Max(x => x.ClientId) + 1,
                        ClientPhone = orderdto.clientPhone,
                        ClientEmail = orderdto.clientEmail,
                        ClientCountoforders = 1,
                        ClientName = orderdto.clientName
                    };
                    DataBaseConnection.Context.Clients.Add(client1);
                    orderDTO.ClientPhone = orderdto.clientPhone;
                    orderDTO.OrderId = DataBaseConnection.Context.Orders.Max(x => x.OrderId) + 1;
                    AnimalCheck(client1.ClientPhone);
                    orderDTO.OrderStatusId = 2;
                    orderDTO.Totalprice = orderdto.Totalprice;
                    orderDTO.AdmissionDate = orderdto.admDate;
                    orderDTO.IssueDate = orderdto.issueDate;
                    orderDTO.RoomId = DataBaseConnection.Context.Rooms.ToList().FirstOrDefault(x => x.RoomNumber == orderdto.roomId).RoomId;
                    Room selroom = DataBaseConnection.Context.Rooms.FirstOrDefault(x => x.RoomId == orderDTO.RoomId);
                    selroom.RoomStatusid = 2;

                    DataBaseConnection.Context.Orders.Add(OrdergetMapper.ConvertToOrder(orderDTO));
                    DataBaseConnection.Context.Rooms.Update(selroom);
                    DataBaseConnection.Context.SaveChanges();
                    MailAddress from = new MailAddress("chunowalow@gmail.com", "AnimalHouse");
                    // кому отправляем
                    MailAddress to = new MailAddress($"{orderdto.clientEmail}");
                    // создаем объект сообщения
                    MailMessage m = new MailMessage(from, to);
                    // тема письма
                    m.Subject = $"Заявка на проживание #{orderDTO.OrderId}";
                    // текст письма
                    m.Body = $"<a href=\"http://localhost:8081/orders/GenerateDoc?id={orderDTO.OrderNoteId}\">Скачать Квитанцию<a/>" +
                        $"<br/>" +
                        $"<a href=\"http://localhost:8081/orders/GenerateAgreement?id={orderDTO.OrderNoteId}\">Скачать договор<a/>";
                    // письмо представляет код html
                    m.IsBodyHtml = true;
                    // адрес smtp-сервера и порт, с которого будем отправлять письмо
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    // логин и пароль
                    smtp.Credentials = new NetworkCredential("chunowalow@gmail.com", "xpdr zpdd sdwj smnd");
                    smtp.EnableSsl = true;
                    smtp.Send(m);
                    return Ok();
                }
            }
        }

        [HttpGet("/orders/GenerateDoc")]
        public IActionResult CreateDoc([FromQuery] int id)
        {
            var order = DataBaseConnection.Context.Orders.FirstOrDefault(x => x.OrderId == id);
            var client = DataBaseConnection.Context.Clients.FirstOrDefault(x => x.ClientPhone == order.ClientPhone);
            Stream stream = new MemoryStream();
            string filepath = "assets/paydoc.xlsx";
            if (!System.IO.File.Exists(filepath))
            {
                return NotFound("Файл не найден.");
            }

            using (var workbook = new XLWorkbook(filepath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    foreach (var cell in worksheet.CellsUsed())
                    {
                        if (cell.Value.ToString().Contains("CLIENTNAME"))
                        {
                            cell.Value = cell.Value.ToString().Replace("CLIENTNAME", client.ClientName);
                        }
                        else if (cell.Value.ToString().Contains("ORDERID"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ORDERID", order.OrderId.ToString());
                        }
                        else if (cell.Value.ToString().Contains("CLIENTPHONE"))
                        {
                            cell.Value = cell.Value.ToString().Replace("CLIENTPHONE", client.ClientPhone.ToString());
                        }
                        else if (cell.Value.ToString().Contains("ADMDATE"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ADMDATE", order.AdmissionDate.Value.ToString("d"));
                        }
                        else if (cell.Value.ToString().Contains("ISSUEDATE"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ISSUEDATE", order.IssueDate.Value.ToString("d"));
                        }
                        else if (cell.Value.ToString().Contains("CLIENTMAIL"))
                        {
                            cell.Value = cell.Value.ToString().Replace("CLIENTMAIL", client.ClientEmail);
                        }
                        else if (cell.Value.ToString().Contains("ANIMALNAME"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ANIMALNAME", order.Animal.AnimalName);
                        }
                        else if (cell.Value.ToString().Contains("ANIMALGEN"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ANIMALGEN", order.Animal.AnimalGen);
                        }
                        else if (cell.Value.ToString().Contains("ANIMALTYPE"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ANIMALTYPE", order.Animal.AnimalBreed.AnimalType.AnimaltypeName);
                        }
                        else if (cell.Value.ToString().Contains("ANIMALBREED"))
                        {
                            cell.Value = cell.Value.ToString().Replace("ANIMALBREED", order.Animal.AnimalBreed.AnimalbreedName);
                        }
                        else if (cell.Value.ToString().Contains("TOTALDAYS"))
                        {
                            cell.Value = cell.Value.ToString().Replace("TOTALDAYS", (order.IssueDate.Value.Day - order.AdmissionDate.Value.Day).ToString());
                        }
                        else if (cell.Value.ToString().Contains("TOTALPRICE"))
                        {
                            cell.Value = cell.Value.ToString().Replace("TOTALPRICE", order.Totalprice.ToString());
                        }
                        else if (cell.Value.ToString().Contains("ROOMNUMBER"))
                        {
                            cell.Value.ToString().Replace("ROOMNUMBER", order.RoomId.ToString());
                        }
                    }
                }
                workbook.SaveAs(stream);
                stream.Position = 0;
                return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }

            
            
        }
        [HttpGet("/orders/GenerateAgreement")]
        public IActionResult GenerateAgreement([FromQuery] int id)
        {
            var order = DataBaseConnection.Context.Orders.FirstOrDefault(x => x.OrderId == id);
            var client = DataBaseConnection.Context.Clients.FirstOrDefault(x => x.ClientPhone == order.ClientPhone);

            if (order == null || client == null)
            {
                return NotFound("Заказ или клиент не найдены.");
            }

            string filepath = "assets/agreement.docx";
            if (!System.IO.File.Exists(filepath))
            {
                return NotFound("Файл не найден.");
            }

            using (var stream = new MemoryStream())
            {
                using (var doc = DocX.Load(filepath))
                {
                    // Заменяем метки на фактические данные
                    doc.ReplaceText("CLIENTNAME", client.ClientName);
                    doc.ReplaceText("ORDERID", order.OrderId.ToString());
                    doc.ReplaceText("ANIMALNAME", order.Animal.AnimalName);
                    doc.ReplaceText("ADMISSIONDATE", order.AdmissionDate.ToString());
                    doc.ReplaceText("ISSUEDATE", order.IssueDate.ToString());
                    doc.ReplaceText("TOTALPRICE", order.Totalprice.ToString());
                    doc.ReplaceText("CLIENTPHONE", order.ClientPhone.ToString());

                    doc.SaveAs(stream);
                }

                stream.Position = 0;
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "agreement.docx");
            }
        }
    }
}