using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IExtraitService
    {
        /// <summary>
        /// Cette méthode retourne un extrait suivant l'id reçu.
        /// </summary>
        /// <returns>Retourne un extrait suivant l'id reçu</returns>
        public Task<Extrait?> Get(int extraitId);
        /// <summary>
        /// Cette méthode retourne un extrait avec les informations de l'utilisateur qui l'a ajouté
        /// </summary>
        /// <returns>Retourne un extrait avec les informations de l'utilisateur qui l'a ajouté</returns>
        public Task<Extrait?> GetWithUserAndEvent(int extraitId);
        /// <summary>
        /// Cette méthode retourne tous les extraits d'un événement suivant l'id de l'événement reçu.
        /// </summary>
        /// <returns>Retourne tous les extraits d'un événement suivant l'id de l'événement reçu</returns>
        public Task<List<Extrait>> GetExtraitsByEvent(int eventId);
        /// <summary>
        /// Cette méthode ajoute un extrait à la base de données.
        /// </summary>
        /// </returns> <returns>Retourne l'extrait ajouté à la base de données</returns>
        public Task<Extrait> Add(Extrait extrait);
        /// <summary>
        /// Cette méthode supprime un extrait de la base de données.
        /// </summary>
        /// <returns>Retourne l'extrait supprimé de la base de données</returns>
        public Task<Extrait> Delete(Extrait extrait);
        /// <summary>
        /// Cette méthode met à jour un extrait dans la base de données.
        /// </summary>
        /// <returns>Retourne l'extrait mis à jour dans la base de données</returns>
        public Task UpdateExtrait(Extrait extrait);
    }
}
