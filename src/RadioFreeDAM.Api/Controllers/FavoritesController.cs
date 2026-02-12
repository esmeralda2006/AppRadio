using Microsoft.AspNetCore.Mvc;
using RadioFreeDAM.Api.Data.Entities;
using RadioFreeDAM.Api.Data.Repositories;

namespace RadioFreeDAM.Api.Controllers;

[ApiController]
[Route("api/favorites")]
public class FavoritesController : ControllerBase
{
    private readonly FavoriteRepository _favoriteRepository;

    public FavoritesController(FavoriteRepository favoriteRepository)
    {
        _favoriteRepository = favoriteRepository;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetFavorites(int userId)
    {
        try 
        {
            var favs = await _favoriteRepository.GetByUserIdAsync(userId);
            return Ok(favs);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAVS-GET] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al obtener favoritos" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite(FavoriteEntity fav)
    {
        try 
        {
            var exists = await _favoriteRepository.ExistsAsync(fav.UserId, fav.StationId);

            if (exists)
            {
                return Conflict(new { message = "Esta emisora ya está en favoritos." });
            }

            await _favoriteRepository.AddAsync(fav);
            return Ok(new { message = "Añadido a favoritos" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAVS-ADD] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al añadir favorito" });
        }
    }

    [HttpDelete("{userId}/{stationId}")]
    public async Task<IActionResult> RemoveFavorite(int userId, string stationId)
    {
        try 
        {
            await _favoriteRepository.DeleteByCompositeKeyAsync(userId, stationId);
            return Ok(new { message = "Eliminado de favoritos" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAVS-DEL] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al eliminar favorito" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFavorite(int id)
    {
        try 
        {
            await _favoriteRepository.DeleteAsync(id);
            return Ok(new { message = "Eliminado de favoritos" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[FAVS-DEL-ID] Error: {ex.Message}");
            return StatusCode(500, new { message = "Error al eliminar favorito" });
        }
    }
}
