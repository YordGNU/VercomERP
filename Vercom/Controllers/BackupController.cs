using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vercom.Services;

namespace Vercom.Controllers;

[Authorize(Roles = "MASTER,ADMINISTRADOR")]
public class BackupController : Controller
{
    private readonly IBackupService _backupService;

    public BackupController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public async Task<IActionResult> Index()
    {
        var logs = await _backupService.GetBackupLogsAsync();
        return View(logs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create()
    {
        var result = await _backupService.CreateBackupAsync();
        if (result.Success)
            TempData["Success"] = result.Message;
        else
            TempData["Error"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Download(Guid id)
    {
        var log = await _backupService.GetBackupLogByIdAsync(id);
        if (log == null || !System.IO.File.Exists(log.RutaArchivo))
            return NotFound();

        var fileName = Path.GetFileName(log.RutaArchivo);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(log.RutaArchivo);
        return File(fileBytes, "application/octet-stream", fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(Guid id)
    {
        var result = await _backupService.RestoreBackupAsync(id);
        if (result.Success)
        {
            TempData["Success"] = result.Message;
            return RedirectToAction("Logout", "Account");
        }
        else
        {
            TempData["Error"] = result.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleanOldBackups(int daysToKeep = 30)
    {
        var deletedCount = await _backupService.DeleteOldBackupsAsync(daysToKeep);
        TempData["Success"] = $"Se eliminaron {deletedCount} respaldos antiguos (más de {daysToKeep} días).";
        return RedirectToAction(nameof(Index));
    }
}