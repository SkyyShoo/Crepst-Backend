using Backend.Models;
using Backend.Models.DTOs;

namespace Backend.Services.Interfaces
{
    public interface IEventService
    {
        /// <summary>
        /// Cette méthode retourne la liste des cercles de lecture (events) à venir en ordre croissant de date.
        /// </summary>
        /// <returns>Retourne une liste de cercles de lecture (events) en ordre croissant de date</returns>
        public Task<IEnumerable<EventDTO>> GetEvents();

        /// <summary>
        /// Cette méthode returne un cercle de lecture (event) correspondant au id envoyer dans les paramètres
        /// </summary>
        /// <param name="eventId">L'id du cercle de lecture que l'on souhaite récupérer</param>
        /// <returns>Retourne un cercle de lecture (event)</returns>
        public Task<Event?> Get(int eventId);

        /// <summary>
        /// Cette méthode retourne le prochain cercle de lecture (event) à venir.
        /// </summary>
        /// <returns>Retourne le prochain cercle de lecture (event)</returns>
        public Task<EventDTO?> Next();

        /// <summary>
        /// Cette méthode modifie le cercle de lecture (event) à venir en fonction des paramètres envoyer et retourne le cercle de lecture (event) modifié.
        /// </summary>
        /// <returns>Retourne le cercle de lecture (event) modifié</returns>
        public Task<Event?> Update(Event @event, EventDTO @eventDTO);

        /// <summary>
        /// Cette méthode créer le cercle de lecture (event) en fonction des paramètres envoyer et retourne le cercle de lecture (event) créé.
        /// </summary>
        /// <returns>Retourne le cercle de lecture (event) créé</returns>
        public Task<Event?> Create(EventDTO @eventDTO);

        /// <summary>
        /// Cette méthode supprime le cercle de lecture (event) en fonction des paramètres envoyer.
        /// </summary>
        /// <returns>Retourne le cercle de lecture (event) créé</returns>
        public Task<Event?> Delete(int id);

        /// <summary>Met à jour le fichier résumé d'un cercle de lecture.</summary>
        public Task<Event?> UpdateResumeFile(int id, string? fileName, string? mimeType);
    }
}
