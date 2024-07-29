using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoNumerationFabricationParts_R2022.Extensions
{
    public static class DocumentExtensionRG
    {

        public static List<Element> GetAllElementsInDocument(this Document document)
        {
            var elements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            return elements;
        }

        //create method get element by its type name
        public static List<Element> GetElementsByTypeName(this Document document, string typeName)
        {
            var elements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .ToElements()
                .Where(e => e.GetType().Name == typeName)
                .ToList();

            return elements;
        }

        public static Material GetMaterialByName(this Document document, string materialName)
        {
            var material = new FilteredElementCollector(document)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(m => m.Name == materialName);

            return material;
        }
        public static Parameter GetParameterByName(this Document document, string parameterName)
        {
            var parameter = new FilteredElementCollector(document)
                .OfClass(typeof(ParameterElement))
                .Cast<ParameterElement>()
                .SelectMany(p => p.GetOrderedParameters())
                .FirstOrDefault(p => p.Definition.Name == parameterName);

            return parameter;
        }

        public static List<Element> GetTypesVisibleInView(this Document document, View view)
        {
            var elements = new FilteredElementCollector(document, view.Id)
                .Distinct()
                .ToList();

            return elements;
        }

        //add extension method to get all possible material names
        public static List<string> GetAllMaterials(this Document document)
        {
            var materials = new FilteredElementCollector(document)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .Select(m => m.Name)
                .ToList();
            materials.Sort();

            return materials;
        }

        /// <summary>
        /// It returns elements by a given type
        /// </summary>
        /// <typeparam name="TElement">The element type</typeparam>
        /// <param name="document">Document</param>
        /// <param name="validate">A delegate for validation</param>
        /// <returns>Elements</returns>
        public static List<TElement> GetElementsByType<TElement>(this Document document, Func<TElement, bool> validate = null)
            where TElement : Element
        {
            validate = validate ?? (e => true);
            var elements = new FilteredElementCollector(document)
                .OfClass(typeof(TElement))
                .Cast<TElement>()
                .Where(e => validate(e))
                .ToList();
            return elements;
        }


        /// <summary>
        /// It returns an element by a given name
        /// </summary>
        /// <typeparam name="TElement">The element type</typeparam>
        /// <param name="document">Document</param>
        /// <param name="name">The element name</param>
        /// <returns>The element</returns>
        /// <exception cref="ArgumentNullException">Thrown in case of the absence of the element</exception>
        public static TElement GetElementByName<TElement>(this Document document, string name)
            where TElement : Element
        {
            var element = new FilteredElementCollector(document)
                .OfClass(typeof(TElement))
                .FirstOrDefault(e => e.Name == name);
            if (element is null)
                throw new ArgumentNullException($"The element of the given name : {name} is not present in a document");
            return element as TElement;
        }


        /// <summary>
        /// Returns the elements by types
        /// </summary>
        /// <typeparam name="TElement1">The first element type</typeparam>
        /// <typeparam name="TElement2">The second element type</typeparam>
        /// <param name="document">Document</param>
        /// <returns>The elements</returns>
        public static List<Element> GetElementsByTypes<TElement1, TElement2>(
            this Document document)
            where TElement1 : Element
            where TElement2 : Element
        {
            var types = new List<Type>()
            {
                typeof(TElement1),
                typeof(TElement2)
            };
            var multiClassFilter = new ElementMulticlassFilter(types);
            return new FilteredElementCollector(document)
                .WherePasses(multiClassFilter).ToList();
        }

        /// <summary>
        /// Returns the elements by types
        /// </summary>
        /// <typeparam name="TElement1">The first element type</typeparam>
        /// <typeparam name="TElement2">The second element type</typeparam>
        /// <typeparam name="TElement3">The third element type</typeparam>
        /// <param name="document"></param>
        /// <returns>The elements</returns>
        public static List<Element> GetElementsByTypes<TElement1, TElement2, TElement3>(
            this Document document)
            where TElement1 : Element
            where TElement2 : Element
            where TElement3 : Element
        {
            var types = new List<Type>()
            {
                typeof(TElement1),
                typeof(TElement2),
                typeof(TElement3),
            };
            var multiClassFilter = new ElementMulticlassFilter(types);
            return new FilteredElementCollector(document)
                .WherePasses(multiClassFilter).ToList();
        }


        /// <summary>
        /// Returns the elements by types
        /// </summary>
        /// <typeparam name="TElement1">The first element type</typeparam>
        /// <typeparam name="TElement2">The second element type</typeparam>
        /// <typeparam name="TElement3">The third element type</typeparam>
        /// <typeparam name="TElement4">The forth element type</typeparam>
        /// <param name="document"></param>
        /// <returns>The elements</returns>
        public static List<Element> GetElementsByTypes<TElement1, TElement2, TElement3, TElement4>(
            this Document document)
            where TElement1 : Element
            where TElement2 : Element
            where TElement3 : Element
            where TElement4 : Element
        {
            var types = new List<Type>()
            {
                typeof(TElement1),
                typeof(TElement2),
                typeof(TElement3),
                typeof(TElement4)
            };
            var multiClassFilter = new ElementMulticlassFilter(types);
            return new FilteredElementCollector(document)
                .WherePasses(multiClassFilter).ToList();
        }
        /// <summary>
        /// It returns elements by types
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="types">Types</param>
        /// <returns>The elements</returns>
        public static List<Element> GetElementsByTypes(
            this Document document,
            params Type[] types)
        {
            if (!types.Any()) throw new ArgumentNullException("There are no types");
            var multiClassFilter = new ElementMulticlassFilter(types);
            return new FilteredElementCollector(document)
                .WherePasses(multiClassFilter).ToList();
        }

        /// <summary>
        /// It returns elements by types
        /// </summary>
        /// <param name="document">Document</param>
        /// <param name="types">Types</param>
        /// <returns>The elements</returns>
        public static List<Element> GetElementsByTypes(
            this Document document,
            List<Type> types)
        {
            var multiClassFilter = new ElementMulticlassFilter(types);
            return new FilteredElementCollector(document)
                .WherePasses(multiClassFilter).ToList();
        }

        public static void Run(
            this Document document, Action doAction, string transactionName = "Default transaction name")
        {
            using (var transaction = new Transaction(document, transactionName))
            {
                transaction.Start();
                doAction.Invoke();
                transaction.Commit();
            }
        }

        public static TReturn Run<TReturn>(
            this Document document, Func<TReturn> doAction, string transactionName = "Default transaction name")
        {
            TReturn output;
            using (var transaction = new Transaction(document, transactionName))
            {
                transaction.Start();
                output = doAction.Invoke();
                transaction.Commit();
            }

            return output;
        }

        public static TElement GetElement<TElement>(this Document document, ElementId elementId)
        where TElement : Element
        {
            return document.GetElement(elementId) as TElement;
        }

        /// <summary>
        /// This method is used to create direct shapes in a Revit Document
        /// </summary>
        /// <param name="document"></param>
        /// <param name="geometryObjects"></param>
        /// <param name="builtInCategory"></param>
        /// <returns></returns>
        public static DirectShape CreateDirectShape(
            this Document document,
            IEnumerable<GeometryObject> geometryObjects,
            BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {

            var directShape = DirectShape.CreateElement(document, new ElementId(builtInCategory));
            directShape.SetShape(geometryObjects.ToList());
            return directShape;

        }

        public static DirectShape CreateDirectShape(
            this Document document,
            GeometryObject geometryObject,
            BuiltInCategory builtInCategory = BuiltInCategory.OST_GenericModel)
        {

            var directShape = DirectShape.CreateElement(document, new ElementId(builtInCategory));
            directShape.SetShape(new List<GeometryObject>() { geometryObject });
            return directShape;
        }

        public static List<InternalDefinition> GetAllDefinitions(
            this Document document)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));
            var outputDefinitions = new List<InternalDefinition>();
            var iterator = document.ParameterBindings.ForwardIterator();
            while (iterator.MoveNext())
            {
                outputDefinitions.Add(iterator.Key as InternalDefinition);
            }
            return outputDefinitions;
        }

        public static InternalDefinition GetDefinitionByName(
            this Document document,
            string definitionName)
        {
            if (document is null) throw new ArgumentNullException(nameof(document));
            if (definitionName is null) throw new ArgumentNullException(nameof(definitionName));
            var iterator = document.ParameterBindings.ForwardIterator();
            while (iterator.MoveNext())
            {
                var definition = iterator.Key as InternalDefinition;
                if (definition?.Name == definitionName) return definition;
            }

            throw new ArgumentNullException($"The definition of the given name {definitionName} does not exist");
        }
    }
}
