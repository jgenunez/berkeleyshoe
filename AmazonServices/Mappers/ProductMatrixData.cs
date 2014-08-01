using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BerkeleyEntities;

namespace AmazonServices
{
    public abstract class ProductMatrixData
    {
        protected ItemClass _itemClass;

        public abstract Product GetProductMatrixDto(string title);

        public abstract Relationship GetRelationshipDto(int marketplaceID);

    }
}
