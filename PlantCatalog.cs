namespace FinalYearProject
{
    public sealed record CatalogItem(
        string Name,
        string Kind,
        string Family,
        string Season,
        int Litres,
        int IntervalDays,
        string Sun,
        string Soil,
        string Tip)
    {
        public string Summary => $"{Season} · {Litres} L every {IntervalDays} days";
        public string Initial => Name[..1];
    }

    public static class PlantCatalog
    {
        public static readonly IReadOnlyList<CatalogItem> Fruits = new List<CatalogItem>
        {
            new("Apple", "Fruit", "Rose", "Autumn", 15, 7, "Full sun", "Well-drained loam", "Prune in late winter and thin fruit in June for bigger apples."),
            new("Strawberry", "Fruit", "Rose", "Summer", 3, 2, "Full sun", "Rich, slightly acidic", "Mulch with straw to keep berries clean and the soil moist."),
            new("Raspberry", "Fruit", "Rose", "Summer", 5, 3, "Full sun", "Moist, well-drained", "Cut fruited canes to the ground after harvest."),
            new("Blueberry", "Fruit", "Heath", "Summer", 4, 3, "Full sun", "Acidic (pH 4.5-5.5)", "Use ericaceous compost and rainwater where possible."),
            new("Grapes", "Fruit", "Vine", "Autumn", 10, 10, "Full sun", "Light, well-drained", "Train along wires and prune hard in winter."),
            new("Lemon", "Fruit", "Citrus", "Winter", 6, 5, "Full sun", "Free-draining", "Bring pots indoors when temperatures drop below 5 °C."),
            new("Orange", "Fruit", "Citrus", "Winter", 8, 5, "Full sun", "Free-draining", "Feed monthly with a citrus fertiliser in the growing season."),
            new("Banana", "Fruit", "Banana", "All year", 20, 3, "Full sun", "Rich, moist", "Needs shelter from wind and plenty of water in summer."),
            new("Mango", "Fruit", "Cashew", "Summer", 18, 7, "Full sun", "Deep, sandy loam", "Water deeply but let the topsoil dry between waterings."),
            new("Pineapple", "Fruit", "Bromeliad", "Summer", 3, 7, "Full sun", "Sandy, acidic", "Can be regrown from the leafy top of a shop-bought fruit."),
            new("Watermelon", "Fruit", "Gourd", "Summer", 12, 3, "Full sun", "Sandy, warm", "Water at the base; stop watering as fruit ripens for sweeter flavour."),
            new("Pear", "Fruit", "Rose", "Autumn", 15, 7, "Full sun", "Deep, moist loam", "Pick slightly early and ripen indoors for the best texture."),
            new("Cherry", "Fruit", "Rose", "Summer", 12, 7, "Full sun", "Well-drained", "Net the tree before fruit colours to protect it from birds."),
            new("Peach", "Fruit", "Rose", "Summer", 14, 7, "Full sun", "Light, well-drained", "Shelter from frost while blossoming in early spring."),
            new("Fig", "Fruit", "Mulberry", "Autumn", 8, 7, "Full sun", "Poor to average, free-draining", "Restrict the roots in a large pot to encourage fruiting."),
        };

        public static readonly IReadOnlyList<CatalogItem> Vegetables = new List<CatalogItem>
        {
            new("Tomatoes", "Vegetable", "Nightshade", "Summer", 8, 2, "Full sun", "Rich, well-drained", "Pinch out side shoots and feed weekly once flowering."),
            new("Peppers", "Vegetable", "Nightshade", "Summer", 5, 3, "Full sun", "Rich, well-drained", "Keep warm; harvest regularly to encourage more fruit."),
            new("Potatoes", "Vegetable", "Nightshade", "Summer", 10, 4, "Full sun", "Loose, slightly acidic", "Earth up stems as they grow to prevent green tubers."),
            new("Carrots", "Vegetable", "Carrot", "Spring", 4, 3, "Full sun", "Light, stone-free", "Sow thinly and thin seedlings to avoid forked roots."),
            new("Lettuce", "Vegetable", "Daisy", "Spring", 3, 2, "Part shade", "Moist, fertile", "Sow a few seeds every two weeks for continuous harvests."),
            new("Spinach", "Vegetable", "Amaranth", "Spring", 3, 3, "Part shade", "Rich, moist", "Bolts in heat, so grow in cooler months or partial shade."),
            new("Cabbage", "Vegetable", "Brassica", "Autumn", 6, 4, "Full sun", "Firm, fertile", "Use netting against cabbage white butterflies."),
            new("Broccoli", "Vegetable", "Brassica", "Autumn", 6, 4, "Full sun", "Firm, fertile", "Harvest the main head before the flowers open."),
            new("Peas", "Vegetable", "Legume", "Spring", 4, 3, "Full sun", "Light, well-drained", "Provide supports early; pick often for tender pods."),
            new("Beans", "Vegetable", "Legume", "Summer", 5, 3, "Full sun", "Rich, well-drained", "Fix nitrogen in the soil, which makes them ideal before heavy feeders."),
            new("Onions", "Vegetable", "Allium", "Summer", 3, 5, "Full sun", "Light, well-drained", "Stop watering when tops fall over, then dry the bulbs."),
            new("Cucumber", "Vegetable", "Gourd", "Summer", 8, 2, "Full sun", "Rich, moist", "Train upwards on a trellis to save space and improve airflow."),
        };

        public static IEnumerable<CatalogItem> All => Fruits.Concat(Vegetables);

        public static CatalogItem? Find(string? name) =>
            All.FirstOrDefault(i => string.Equals(i.Name, name?.Trim(), StringComparison.OrdinalIgnoreCase));

        // Crops that share a family should not follow each other in the same bed.
        public static string RotationAdvice(string? previous, string? next)
        {
            var prev = Find(previous);
            var nxt = Find(next);
            if (prev is null || nxt is null)
            {
                return "Add crops to your beds to get rotation advice.";
            }

            if (prev.Family == nxt.Family)
            {
                return $"Avoid following {prev.Name} with {nxt.Name}: same family ({prev.Family}), which builds up pests and disease.";
            }

            if (prev.Family == "Legume")
            {
                return $"{prev.Name} fix nitrogen, so {nxt.Name} is a great follow-on.";
            }

            return $"{nxt.Name} after {prev.Name} is a sound rotation.";
        }
    }
}
