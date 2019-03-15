using System.Collections.Generic;

namespace Catalog.DataAccessLayer.Catalog.Entity
{
    public class MemberRole
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<MemberInfoRoleReference> MemberInfoReferences { get; set; }
    }
}