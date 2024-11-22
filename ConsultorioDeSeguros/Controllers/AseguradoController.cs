using ConsultorioDeSeguros.Models;
using ConsultorioDeSeguros.Persistences.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ConsultorioDeSeguros.Controllers
{
    public class AseguradoController : Controller
    {
        private readonly IAseguradoRepository _aseguradoRepository;

        public AseguradoController(IAseguradoRepository aseguradoRepository)
        {
            _aseguradoRepository = aseguradoRepository;
        }


        public async Task<IActionResult> Index(string? cedula)
        {
            IEnumerable<Asegurado> asegurados;

            if (cedula != null)
            {
                asegurados = await _aseguradoRepository.GetByCiAsync(cedula!);
            }
            else {
                asegurados = await _aseguradoRepository.GetAllAsync();
            }

            return View(asegurados);
        }


        public async Task<IActionResult> Details(int id)
        {
            var asegurado = await _aseguradoRepository.GetById(id);
            if (asegurado == null)
            {
                return NotFound();
            }
            return View(asegurado);
        }

  
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Asegurado asegurado)
        {
            await _aseguradoRepository.RegisterAsync(asegurado);


            if (asegurado.Id > 0)
            {

                await _aseguradoRepository.AsignarSegurosPorEdad(asegurado);
            }
            else
            {
                throw new InvalidOperationException("El asegurado no se ha guardado correctamente en la base de datos.");
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var asegurado = await _aseguradoRepository.GetById(id);
            if (asegurado == null)
            {
                return NotFound();
            }
            return View(asegurado);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Asegurado asegurado)
        {
            if (id != asegurado.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                await _aseguradoRepository.EditAsync(asegurado);
                await _aseguradoRepository.AsignarSegurosPorEdad(asegurado);
                return RedirectToAction(nameof(Index));
            }
            return View(asegurado);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var asegurado = await _aseguradoRepository.GetById(id);
            if (asegurado == null)
            {
                return NotFound();
            }

            return View(asegurado);
        }


        [HttpPost, ActionName("Delete")]        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _aseguradoRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        public async Task<IActionResult> CargarData(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _aseguradoRepository.CargarData(file);
                return RedirectToAction(nameof(Index));
            }
            return BadRequest("No se ha cargado ningún archivo.");
        }
    }
}
