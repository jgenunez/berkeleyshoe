using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BerkeleyEntities
{
    public class Attribute
    {
        public string Code { get; set; }

        public string Value { get; set; }
    }

    public class ProductFactory
    {
        protected berkeleyEntities _dataContext;
        protected Item _item;
        
        public ProductFactory(berkeleyEntities dataContext)
        {
            _dataContext = dataContext;
        }

        public Item GetProduct(string sku)
        {
            _item = _dataContext.Items.Single(p => p.ItemLookupCode.Equals(sku));

            _item.Attributes = new Dictionary<string, Attribute>();

            string deptCode = _item.Department.code;

            switch(deptCode)
            {
                case "31387" : 
                    this.CreateWristwatch(); break;

                case "45246" :
                case "155189" : 
                case "79720":
                    this.CreateSunglasses(); break;

                case "11632" : 
                case "62107" :
                case "53548" :
                case "55793" :
                case "45333" :
                case "53557" :
                case "95672" :
                case "155202" :
                case "155196" :
                case "57929" :
                case "147285" :
                case "11505" :
                case "11504" :
                case "11501" :
                case "53120" :
                case "24087" :
                case "11498" :
                case "15709" :
                case "57974" :
                    this.CreateShoes(); break;

                case "3002" :
                    this.CreateBlazers(); break;

                case "57988" :
                    this.CreateJacket(); break;

                case "57990":
                    this.CreateShirt(); break;

                case "57991" :
                    this.CreateDressShirt(); break;

                case "11483" :
                    this.CreateJeans(); break;

                case "57989" :
                    this.CreatePants(); break;

                case "15689":
                    this.CreateShorts(); break;

                case "11507" :
                    this.CreateUnderwear(); break;

                case "57735" :
                    this.CreateTShirt(); break;

                case "11484":
                    this.CreateSweater(); break;

                case "11848" :
                case "29585" :
                    this.CreateFragances(); break;
            }

            return _item;
        }

        protected virtual void CreateWristwatch()
        {
        }

        protected virtual void CreateSunglasses()
        {
        }

        protected virtual void CreateFragances()
        {

        }

        protected virtual void CreateSweater()
        {
            this.CreateShirt();
        }

        protected virtual void CreateJacket()
        {
            this.CreateShirt();
        }

        protected virtual void CreateBlazers()
        {
            this.CreateShirt();
        }

        protected virtual void CreateTShirt()
        {
            this.CreateShirt();
        }

        protected virtual void CreateDressShirt()
        {
            this.CreateShirt();
        }

        protected virtual void CreateShirt()
        {
            if (_item.ItemClass != null)
            {
                ItemClassComponent component = _item.ItemClassComponents.First();
                ItemClass itemClass = _item.ItemClass;

                string sizeCode, colorCode;

                switch (itemClass.Dimensions)
                {
                    case 1:
                        sizeCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        _item.Attributes.Add("Size", new Attribute() { Code = sizeCode, Value = component.Detail1 }); break;

                    case 2:
                        colorCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        sizeCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        _item.Attributes.Add("Color", new Attribute() { Code = colorCode, Value = component.Detail1 });
                        _item.Attributes.Add("Size", new Attribute() { Code = sizeCode, Value = component.Detail2 }); break;
                }
            }
            else
            {
                string[] skuDetails = _item.ItemLookupCode.Split(new Char[1] { '-' });

                switch (skuDetails.Length)
                {
                    case 2:
                        _item.Attributes.Add("Size", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] }); break;
                }
            }
        }

        protected virtual void CreateJeans()
        {
            this.CreatePants();
        }

        protected virtual void CreatePants()
        {
            if (_item.ItemClass != null)
            {
                ItemClassComponent component = _item.ItemClassComponents.First();
                ItemClass itemClass = _item.ItemClass;

                string waistCode, inseamCode, colorCode;

                switch (itemClass.Dimensions)
                {
                    case 1:
                        waistCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        _item.Attributes.Add("Waist", new Attribute() { Code = waistCode, Value = component.Detail1 }); break;

                    case 2:
                        waistCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        inseamCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        _item.Attributes.Add("Waist", new Attribute() { Code = waistCode, Value = component.Detail1 });
                        _item.Attributes.Add("Inseam", new Attribute() { Code = inseamCode, Value = component.Detail2 }); break;

                    case 3:
                        colorCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        waistCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        inseamCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail3) && p.Dimension == 3).Code;
                        _item.Attributes.Add("Color", new Attribute() { Code = colorCode, Value = component.Detail1 });
                        _item.Attributes.Add("Waist", new Attribute() { Code = waistCode, Value = component.Detail2 });
                        _item.Attributes.Add("Inseam", new Attribute() { Code = inseamCode, Value = component.Detail3 }); break;
                }
            }
            else
            {
                string[] skuDetails = _item.ItemLookupCode.Split(new Char[1] { '-' });

                switch (skuDetails.Length)
                {
                    case 2:
                        _item.Attributes.Add("Waist", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] }); break;

                    case 3:
                        _item.Attributes.Add("Waist", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] });
                        _item.Attributes.Add("Inseam", new Attribute() { Code = skuDetails[2], Value = skuDetails[2] }); break;
                }
            }
        }

        protected virtual void CreateShoes()
        {
            if (_item.ItemClass != null)
            {
                ItemClassComponent component = _item.ItemClassComponents.First();
                ItemClass itemClass = _item.ItemClass;

                string sizeCode, widthCode, colorCode;

                switch (itemClass.Dimensions)
                {
                    case 1:
                        sizeCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        _item.Attributes.Add("Size", new Attribute() { Code = sizeCode, Value = component.Detail1 });
                        _item.Attributes.Add("Width", new Attribute() { Code = "M", Value = "M" }); break;


                    case 2:
                        sizeCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        widthCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        _item.Attributes.Add("Size", new Attribute() { Code = sizeCode, Value = component.Detail1 });
                        _item.Attributes.Add("Width", new Attribute() { Code = widthCode, Value = component.Detail2 }); break;

                    case 3:
                        colorCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        sizeCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        widthCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail3) && p.Dimension == 3).Code;
                        _item.Attributes.Add("Color", new Attribute() { Code = colorCode, Value = component.Detail1 });
                        _item.Attributes.Add("Size", new Attribute() { Code = sizeCode, Value = component.Detail2 });
                        _item.Attributes.Add("Width", new Attribute() { Code = widthCode, Value = component.Detail3 }); break;
                }
            }
            else
            {
                string[] skuDetails = _item.ItemLookupCode.Split(new Char[1] { '-' });

                switch (skuDetails.Length)
                {
                    case 2:
                        _item.Attributes.Add("Size", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] });
                        _item.Attributes.Add("Width", new Attribute() { Code = "M", Value = "M" }); break;

                    case 3 :
                        _item.Attributes.Add("Size", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] });
                        _item.Attributes.Add("Width", new Attribute() { Code = skuDetails[2], Value = skuDetails[2] }); break;
                }
            }
        }

        protected virtual void CreateShorts()
        {
            if (_item.ItemClass != null)
            {
                ItemClassComponent component = _item.ItemClassComponents.First();
                ItemClass itemClass = _item.ItemClass;

                string waistCode, colorCode;

                switch (itemClass.Dimensions)
                {
                    case 1:
                        waistCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        _item.Attributes.Add("Waist", new Attribute() { Code = waistCode, Value = component.Detail1 }); break;

                    case 2:
                        colorCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail1) && p.Dimension == 1).Code;
                        waistCode = itemClass.MatrixAttributeDisplayOrders.Single(p => p.Attribute.Equals(component.Detail2) && p.Dimension == 2).Code;
                        _item.Attributes.Add("Color", new Attribute() { Code = colorCode, Value = component.Detail1 });
                        _item.Attributes.Add("Waist", new Attribute() { Code = waistCode, Value = component.Detail2 }); break;
                }
            }
            else
            {
                string[] skuDetails = _item.ItemLookupCode.Split(new Char[1] { '-' });

                switch (skuDetails.Length)
                {
                    case 2:
                        _item.Attributes.Add("Waist", new Attribute() { Code = skuDetails[1], Value = skuDetails[1] }); break;
                }
            }
        }

        protected virtual void CreateUnderwear()
        {
            this.CreateShorts();
        }
    }

}

