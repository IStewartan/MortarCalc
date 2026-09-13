using System.Text.RegularExpressions;

var directionNames = new string[] {
"N","NE","E","SE","S","SW","W","NW"
};

var mortarTable = new List<(double Meters, double Mil)>
{
    (80, 950), (110, 900), (132, 850), (187, 800), (240, 750),
    (290, 700), (340, 650), (385, 600), (430, 550), (470, 500),
    (510, 450), (545, 400), (578, 350), (609, 300), (637, 250),
    (661, 200), (684, 150),
};

var running = true;
Point convertedPlayerPosition = new Point(0, 0);
Point convertedTargetPosition = new Point(0, 0);
RangeAndBearing currentSolution;

Console.WriteLine("Welcome to the Mortar Calculator!");
Console.WriteLine("Enter Player Position");

var player = CollectionPositionData("Player");
convertedPlayerPosition = ConvertPosition(player);

Console.WriteLine("Enter Target Position");
var target = CollectionPositionData("Target");
convertedTargetPosition = ConvertPosition(target);

CalculateRangeAndBearing(convertedPlayerPosition, convertedTargetPosition);
DisplayResults();
Console.WriteLine("\n \nList of Commands:\n P = Change PlayerPosition \n T = Change TargetPosition \n C = Clear Console all previous cordinates are still preserved \n E = Exit");
while (running)
{
    Console.WriteLine("Enter a command:");
    var command = Console.ReadLine()?.ToUpper();
    switch (command)
    {
        case "P":
            Console.WriteLine("Enter Player Position (format: x,y):");
            convertedPlayerPosition = ConvertPosition(CollectionPositionData("Player"));
            break;
        case "T":
            Console.WriteLine("Enter Target Position (format: x,y):");
            convertedTargetPosition = ConvertPosition(CollectionPositionData("Target"));
            break;
        case "E":
            running = false;
            break;
        case "C":
            Console.Clear();
            Console.WriteLine("\n \nList of Commands:\n P = Change PlayerPosition \n T = Change TargetPosition \n C = Clear Console all previous cordinates are still preserved \n E = Exit");
            break;
        default:
            Console.WriteLine("Invalid command. Please try again.");
            break;
    }

    if (CheckEmptyConvertedPosition(convertedPlayerPosition) || CheckEmptyConvertedPosition(convertedTargetPosition))
    {
        Console.WriteLine("Both Player Position and Target Position must be set before calculating.");
        continue;
    }
    else
    {
        CalculateRangeAndBearing(convertedPlayerPosition, convertedTargetPosition);
    }
    DisplayResults();
}

void DisplayResults(){
    Console.WriteLine($"\n{currentSolution.CardinalDirection} Bearing: {Math.Round(currentSolution.BearingDegrees, 2)} degrees, Range: {currentSolution.Range} meters, Mil: {Math.Round(currentSolution.Mil, 0)}");
}

bool CheckEmptyConvertedPosition(Point position)
{
    return position.X == 0 && position.Y == 0;
}
 void CalculateRangeAndBearing(Point playerPosition, Point targetPosition)
{
    var distanceToX = targetPosition.X - playerPosition.X;
    var distanceToY = targetPosition.Y - playerPosition.Y;
    var range = Math.Round(Math.Sqrt(distanceToX * distanceToX + distanceToY * distanceToY) * 100, 2);
    var bearingDegrees = Math.Atan2(distanceToX, distanceToY) * (180 / Math.PI);
    if (bearingDegrees < 0) bearingDegrees += 360;
    var bearingIndex = (int)Math.Round(bearingDegrees / 45.0) % 8;
    var cardinalDirection = directionNames[bearingIndex];

    currentSolution = new RangeAndBearing(range, GetMilFromRange(range, mortarTable), bearingDegrees, cardinalDirection);
}

 double GetMilFromRange(double range, List<(double Meters, double Mil)> table)
{
    for (int i = 0; i < table.Count - 1; i++)
    {
        var lower = table[i];
        var upper = table[i + 1];

        if (range >= lower.Meters && range <= upper.Meters)
        {
            var percentBetween = (range - lower.Meters) / (upper.Meters - lower.Meters);
            return lower.Mil - percentBetween * (lower.Mil - upper.Mil);
        }
    }

    return -1; // range didn't fall inside the table at all — see note below
}

Point ConvertPosition(string position)
{
    var parts = position.Split(',');
    var x = Convert.ToDouble(parts[0].Trim().TrimStart('x'));
    var y = Convert.ToDouble(parts[1].Trim().TrimStart('y'));

    return new Point(x, y);
}

bool IsValid(string input)
{
    var pattern = @"^(?=[^x]*x[^x]*$)(?=[^y]*y[^y]*$)[^,]*x[^,]*,[^,]*y[^,]*$";
    return Regex.IsMatch(input ?? string.Empty, pattern, RegexOptions.IgnoreCase);
}


string CollectionPositionData(string entityType)
{
    var entity = Console.ReadLine();
    do
    {
        if (string.IsNullOrWhiteSpace(entity))
        {
            Console.WriteLine($"{entityType} position cannot be empty. Please enter a valid position.");
            entity = Console.ReadLine();
        }
        else if (!IsValid(entity))
        {
            Console.WriteLine($"{entityType} position is not in a valid format. Please enter a valid position (format: x,y).");
            entity = Console.ReadLine();
        }
    } while (string.IsNullOrWhiteSpace(entity) || !IsValid(entity));
    return   entity ;
}

record struct Point(double X, double Y);
record struct RangeAndBearing(double Range, double Mil, double BearingDegrees, string CardinalDirection);
record struct TargetData(string Description, Point Position);

