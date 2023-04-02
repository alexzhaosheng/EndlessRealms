using EndlessRealms.Core.Utility;
using EndlessRealms.Models;

namespace EndlessRealms.Core.Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestSimpleObject()
    {
        string yamlData = @"
Name: Cyberpunk
Description: The quick brown fox
";
        var world = new YamlLikeParser().Parse<World>(yamlData);
        Assert.That(world.Name, Is.EqualTo("Cyberpunk"));
        Assert.That(world.Description, Is.EqualTo("The quick brown fox"));
    }

    [Test]
    public void TestArray()
    {
        string yamlData = @"
-Name: Cyberpunk
 Description: The quick brown fox
-Name: Wonderland
 Description: jumps over the lazy dog
-Name: FrozenWorld
 Description: It's cold here
";
        var worlds = new YamlLikeParser().Parse<World[]>(yamlData);
        Assert.That(worlds.Length, Is.EqualTo(3));
        Assert.That(worlds[0].Name, Is.EqualTo("Cyberpunk"));        
        Assert.That(worlds[2].Name, Is.EqualTo("FrozenWorld"));
        Assert.That(worlds[0].Description, Is.EqualTo("The quick brown fox"));
        Assert.That(worlds[2].Description, Is.EqualTo("It's cold here"));
    }

    [Test]
    public void TestMixedData()
    {
        string yamlData = @"
Name: Cyberpunk
AdjectiveWords: 
 -Cold
 -Deadly
Description: The quick brown fox
";
        var world = new YamlLikeParser().Parse<World>(yamlData);
        Assert.That(world.Name, Is.EqualTo("Cyberpunk"));
        Assert.That(world.Description, Is.EqualTo("The quick brown fox"));        
        Assert.That(world.AdjectiveWords, Is.EquivalentTo(new[] { "Cold", "Deadly" }));

        yamlData = @"
Name: Cyberpunk
Regions:
 -Name: Night city
  Description: Welcome to the night city
 -Name: Valenhell
  Description: It's velen hell!!
 -Name: Frozen World
  Description: Cold here
";

        world = new YamlLikeParser().Parse<World>(yamlData);
        Assert.That(world.Name, Is.EqualTo("Cyberpunk"));
        Assert.That(world.Regions.Length, Is.EqualTo(3));
        Assert.That(world.Regions[0].Name, Is.EqualTo("Night city"));
        Assert.That(world.Regions[0].Description, Is.EqualTo("Welcome to the night city"));

        Assert.That(world.Regions[2].Name, Is.EqualTo("Frozen World"));
        Assert.That(world.Regions[2].Description, Is.EqualTo("Cold here"));

        yamlData = @"
Name: Cyberpunk
Regions:
 -Name: Night city
  Description: Welcome to the night city
 -Name: Valenhell
  Description: It's velen hell!!
 -Name: Frozen World
  Description: Cold here
Description: The quick brown fox
";
        world = new YamlLikeParser().Parse<World>(yamlData);
        Assert.That(world.Name, Is.EqualTo("Cyberpunk"));
        Assert.That(world.Description, Is.EqualTo("The quick brown fox"));

        Assert.That(world.Regions.Length, Is.EqualTo(3));
        Assert.That(world.Regions[0].Name, Is.EqualTo("Night city"));
        Assert.That(world.Regions[0].Description, Is.EqualTo("Welcome to the night city"));

        Assert.That(world.Regions[2].Name, Is.EqualTo("Frozen World"));
        Assert.That(world.Regions[2].Description, Is.EqualTo("Cold here"));
    }


    [Test]
    public void BadDataTest()
    {
        var yamlData = @"
Response:

Location:
  Description: MysticalAurora is a world of enchantment and wonder, where the stars twinkle brighter than the sun and the forests whisper secrets of the past. It is a realm full of mystery and adventure, where the magical creatures of folklore come alive and roam freely. A harsh and unforgiving desert, where the wind and sand whip like blades and hidden oases offer weary travelers a chance of respite. The sun burns hotter than in any other region, and the air is thick with the smell of spices and sand.
  Things:
    - Name: Fire Dragon
      Number: 6
    - Name: Winged Unicorns
      Number: 10
    - Name: Sand Manta Rays
      Number: 3
    - Name: Fire Salamanders
      Number: 20
";
        var scene = new YamlLikeParser().Parse<Scene>(yamlData);
        Assert.That(scene.Description, Does.Contain("MysticalAurora"));
        Assert.That(scene.Things.Count, Is.EqualTo(4));



        yamlData = @"
Description: CyberVista is an otherworldly world of awe and danger. The land is a vast and turbulent landscape, filled with vibrant colors, towering mountains, and mysterious creatures. Technology and magic are intertwined, with powerful mages wielding cybernetic enhancements and cyborgs utilizing ancient spells. It's a world where the boundaries between reality and fantasy are blurred and the possibilities are endless.

AdjectiveWords: magical, futuristic, surreal, dangerous, vibrant

Characters:
 - FullName: Vanna Lupus
   Title: Sorceress
   Sex: Female
   AgeOfTheCharactor: Unknown
   AppearanceDescription: Vanna is a tall and statuesque woman with long, raven-black hair. Her ethereal beauty is only enhanced by her magical cybernetic enhancements.
   Character: Vanna is a powerful sorceress who is fiercely loyal and brave in the face of danger.
 - FullName: Axel Deadeye
   Title: Bounty Hunter
   Sex: Male
   AgeOfTheCharactor: 35
   AppearanceDescription: Axel is a tall and ruggedly handsome man with a strong build and piercing blue eyes. He wears a black duster and a holstered blaster at his side.
   Character: Axel is a no-nonsense bounty hunter who uses his quick wit and deadly aim to track down his targets.
 - FullName: Kaya Sundancer
   Title: Shaman
   Sex: Female
   AgeOfTheCharactor: 25
   AppearanceDescription: Kaya is a petite and lithe woman with golden-brown skin and sparkling silver eyes. She often wears colorful garments and carries a staff decorated with feathers and beads.
   Character: Kaya is a wise and gentle shaman who has a deep connection to the spiritual world.
 - FullName: Zephyr Stormbringer
   Title: Warrior
   Sex: Female
   AgeOfTheCharactor: 20
   AppearanceDescription: Zephyr is a tall and muscular woman with long, wavy platinum hair and stormy grey eyes. She wears armor made of a combination of metal and leather.
   Character: Zephyr is a brave and daring warrior who fights for justice and equality.
 - FullName: Koda Steelheart
   Title: Mechanic
   Sex: Male
   AgeOfTheCharactor: 30
   AppearanceDescription: Koda is a short and stocky man with a bald head and a gruff demeanor. He wears a jumpsuit and goggles, and is often found tinkering with strange contraptions.
   Character: Koda is a skilled mechanic who has a knack for finding creative solutions to problems.
";
        var world = new YamlLikeParser().Parse<Scene>(yamlData);
        Assert.That(world.Description, Does.Contain("CyberVista is an otherworldly"));        
        Assert.That(world.Characters.Count, Is.EqualTo(5));
    }


    [Test]
    public void BadDataTest2()
    {
        var yamlData = @"
Response:

Location:
  Description: MysticalAurora is a world of enchantment and wonder, where the stars twinkle brighter than the sun and the forests whisper secrets of the past. It is a realm full of mystery and adventure, where the magical creatures of folklore come alive and roam freely. A harsh and unforgiving desert, where the wind and sand whip like blades and hidden oases offer weary travelers a chance of respite. The sun burns hotter than in any other region, and the air is thick with the smell of spices and sand.
  Things:
    - Name: Fire Dragon
      Number: ""6""
    - Name: Winged Unicorns
      Number: ""10""
    - Name: Sand Manta Rays
      Number: ""3""
    - Name: Fire Salamanders
      Number: ""20""
";
        var scene = new YamlLikeParser().Parse<Scene>(yamlData);
        Assert.That(scene.Description, Does.Contain("MysticalAurora"));
        Assert.That(scene.Things.Count, Is.EqualTo(4));
    }

    [Test]
    public void BadDataTest3()
    {
        var yamlData = @"
Description: In front of you, an incredible view stretches out. The majestic mountain peaks reach towards the clouds, and the lush green forests span for miles. Ancient ruins are dotted across the landscape, and in the night sky thousands of stars twinkle with a beautiful light. The air is filled with a magical ambience that can be felt by all who enter this enchanting realm.

Characters:

  - FullName: Eliza Nightstar

    Race: Elf

    Title: Queen of Astaria

    Sex: Female 

    Age: 300

    Appearance: Eliza is a tall and graceful elf with ivory skin and long, silver hair cascading down her back. She wears an elegant dress made of deep purple velvet, adorned with gold embroidery and jewels. Her eyes are a brilliant blue color that twinkle in the starlight. 

    Personality: Wise, kind-hearted, regal and respectful of nature. 

    FriendnessLevel :4
  
Things: 

  - Name : Silver Moonflower 

    Description : A delicate flower with petals of shimmering silver that glows in the moonlight .  

     Number : 3    

     IsIanimate : false  
  
  - Name : Magical Eagle 

      Description : An eagle made out of pure magic , its feathers shimmering like rainbows when it flies through the air .  
      
      Number : 2  
      
      IsIanimate : true
";
        var scene = new YamlLikeParser().Parse<Scene>(yamlData);
        Assert.That(scene.Description, Does.Contain("an incredible view stretches out"));
        Assert.That(scene.Characters.Count, Is.EqualTo(1));
        Assert.That(scene.Characters[0].FullName, Is.EqualTo("Eliza Nightstar"));
        Assert.That(scene.Characters[0].FriendnessLevel, Is.EqualTo(4));
        Assert.That(scene.Things.Count, Is.EqualTo(2));
    }
}