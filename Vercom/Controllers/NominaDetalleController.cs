using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Vercom.Models;
using Vercom.Services;

namespace Vercom.Controllers
{
    [Authorize(Roles = "ADMINISTRADOR,RRHH,DIRECCION")]
    public class NominaDetalleController : Controller
    {
       private readonly AppDbContext _context;   private Guid CurrentEntidadId => Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
        private readonly IPayrollService _payrollService;

        public NominaDetalleController(AppDbContext context, IPayrollService payrollService)
        {
            _context = context;
            _payrollService = payrollService;
        }

        [HttpPost]
        public async Task<IActionResult> Calculate(short anio, short mes)
        {
            var entidadId = Guid.Parse(User.FindFirst("EntidadId")?.Value ?? Guid.Empty.ToString());
            var result = await _payrollService.CalculatePayrollAsync(entidadId, anio, mes);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid periodId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            var result = await _payrollService.ApprovePayrollAsync(periodId, userId);
            if (result.Succeeded) TempData["Success"] = result.Message;
            else TempData["Error"] = result.Message;

            return RedirectToAction(nameof(Index));
        }

        // GET: NominaDetalle
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.NominaDetalles.Include(n => n.Empleado).Include(n => n.PeriodoNomina);
            return View(await appDbContext.ToListAsync());
        }

        // GET: NominaDetalle/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalle = await _context.NominaDetalles
                .Include(n => n.Empleado)
                .Include(n => n.PeriodoNomina)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nominaDetalle == null)
            {
                return NotFound();
            }

            return View(nominaDetalle);
        }

        // GET: NominaDetalle/Create
        public IActionResult Create()
        {
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id");
            ViewData["PeriodoNominaId"] = new SelectList(_context.PeriodoNominas, "Id", "Id");
            return View();
        }

        // POST: NominaDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PeriodoNominaId,EmpleadoId,DiasTrabajados,HorasExtra,SalarioDevengado,TotalDeducciones,SalarioNeto")] NominaDetalle nominaDetalle)
        {
            if (ModelState.IsValid)
            {
                nominaDetalle.Id = Guid.NewGuid();
                _context.Add(nominaDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", nominaDetalle.EmpleadoId);
            ViewData["PeriodoNominaId"] = new SelectList(_context.PeriodoNominas, "Id", "Id", nominaDetalle.PeriodoNominaId);
            return View(nominaDetalle);
        }

        // GET: NominaDetalle/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalle = await _context.NominaDetalles.FindAsync(id);
            if (nominaDetalle == null)
            {
                return NotFound();
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", nominaDetalle.EmpleadoId);
            ViewData["PeriodoNominaId"] = new SelectList(_context.PeriodoNominas, "Id", "Id", nominaDetalle.PeriodoNominaId);
            return View(nominaDetalle);
        }

        // POST: NominaDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,PeriodoNominaId,EmpleadoId,DiasTrabajados,HorasExtra,SalarioDevengado,TotalDeducciones,SalarioNeto")] NominaDetalle nominaDetalle)
        {
            if (id != nominaDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nominaDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NominaDetalleExists(nominaDetalle.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmpleadoId"] = new SelectList(_context.Empleados, "Id", "Id", nominaDetalle.EmpleadoId);
            ViewData["PeriodoNominaId"] = new SelectList(_context.PeriodoNominas, "Id", "Id", nominaDetalle.PeriodoNominaId);
            return View(nominaDetalle);
        }

        // GET: NominaDetalle/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nominaDetalle = await _context.NominaDetalles
                .Include(n => n.Empleado)
                .Include(n => n.PeriodoNomina)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (nominaDetalle == null)
            {
                return NotFound();
            }

            return View(nominaDetalle);
        }

        // POST: NominaDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var nominaDetalle = await _context.NominaDetalles.FindAsync(id);
            if (nominaDetalle != null)
            {
                _context.NominaDetalles.Remove(nominaDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NominaDetalleExists(Guid id)
        {
            return _context.NominaDetalles.Any(e => e.Id == id);
        }
    }
}
