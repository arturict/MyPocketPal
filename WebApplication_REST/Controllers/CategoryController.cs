using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WebApplication_REST.Models;
namespace WebApplication_REST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private List<Category> _categories = new List<Category>(); // Eine temporäre Liste, um Kategoriedaten zu speichern. In der Realität würde dies in einer Datenbank gespeichert.

        [HttpGet]
        public IEnumerable<Category> Get()
        {
            return _categories;
        }

        [HttpPost]
        public ActionResult<Category> Post(Category category)
        {
            try
            {
                // Hier können Sie Validierungen durchführen und die Kategorie in der Datenbank speichern.
                category.Id = Guid.NewGuid(); // Eindeutige ID für die Kategorie.
                _categories.Add(category); // Hinzufügen der Kategorie zur temporären Liste (in der Realität würde dies in der Datenbank erfolgen).
                return Ok(category); // Rückgabe der erstellten Kategorie mit Statuscode 200 (OK).
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Erstellen der Kategorie: " + ex.Message); // Rückgabe eines Fehlerstatuscodes mit einer Fehlermeldung.
            }
        }

        [HttpPut]
        public ActionResult<Category> Put(Category category)
        {
            try
            {
                // Hier können Sie Validierungen durchführen und die Kategorie in der Datenbank aktualisieren.
                var existingCategory = _categories.Find(c => c.Id == category.Id);
                if (existingCategory != null)
                {
                    existingCategory.Name = category.Name;
                    existingCategory.Description = category.Description;
                    // Aktualisieren Sie weitere Kategoriedetails nach Bedarf.
                    return Ok(existingCategory);
                }
                else
                {
                    return NotFound("Kategorie nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Aktualisieren der Kategorie: " + ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(Guid id)
        {
            try
            {
                // Hier können Sie die Kategorie in der Datenbank löschen.
                var existingCategory = _categories.Find(c => c.Id == id);
                if (existingCategory != null)
                {
                    _categories.Remove(existingCategory);
                    return NoContent(); // Erfolgreiche Löschung mit Statuscode 204 (NoContent).
                }
                else
                {
                    return NotFound("Kategorie nicht gefunden");
                }
            }
            catch (Exception ex)
            {
                // Hier können Sie Fehlerbehandlung und Logging hinzufügen.
                return BadRequest("Fehler beim Löschen der Kategorie: " + ex.Message);
            }
        }
    }
}
