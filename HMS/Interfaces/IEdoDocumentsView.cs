using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSystems.Models;

namespace HonestMarkSystem.Interfaces
{
    public interface IEdoDocumentsView
    {
        List<IEdoSystemDocument<string>> GetNewDocuments(Models.ConsignorOrganization myOrganization, out object[] parameters);

        void SaveParameters(Models.ConsignorOrganization myOrganization, params object[] parameters);

        bool SaveNewDocument(Models.ConsignorOrganization myOrganization, IEdoSystemDocument<string> document, out byte[] fileBytes);

        void ChangeMyOrganization();

        void UpdateIdGood();
    }
}
