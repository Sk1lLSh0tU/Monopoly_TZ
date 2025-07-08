using System;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

//Сообщение в меню
string message = """
    -----------------------------------------------------------------------------------------------------------------
    Выберете действие:
    1. Просмотреть базу данных.
    2. Добавить новую паллету.
    3. Добавить новую коробку.
    0. Выход
    """;

while (true)
{
    StartMenu();
}

// Стартовое меню
void StartMenu ()
{

// Вывод меню и получение информации от пользователя
Console.WriteLine(message);
string readUserEntry = Console.ReadLine();

// Определение ввода
switch (readUserEntry)
{
    // Показать базу данных
    case "1":
        
        // Получение базы данных
        string lookAllDB = File.ReadAllText("DB.json");
        List<Pallet> lookAllDBPallet = JsonConvert.DeserializeObject<List<Pallet>>(lookAllDB);

        // Начало обработки базы данных
        sortDB(lookAllDBPallet);
        
        break;

    // Добавить паллету
    case "2":
        Console.WriteLine("Введите Ширину");
        int palletWidth = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите глубину");
        int palletDepth = Convert.ToInt32(Console.ReadLine());

        Pallet newPallet = new Pallet(palletWidth, palletDepth);

        // Сохранить паллету в БД
        SavePalletToDB(newPallet);

        break;

    // Добавить коробку
    case "3":
        Console.WriteLine("Введите название коробки");
        string boxName = Console.ReadLine();

        Console.WriteLine("Введите ширину коробки");
        int boxWidth = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите глубину коробки");
        int boxDepth = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите высоту коробки");
        int boxHeight = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Выберите \n1. Ввести оставшийся срок годности коробки \n2. Ввести дату производства");
        string readChoise = Console.ReadLine();
        
        int ExpDate = 0;
        string currentData = "";
            // Определение Даты и срока годности относительно полученной информации
            if (readChoise == "1") // Если указать срок то расчитываем дату производства
            {
                Console.WriteLine("Введите оставшийся срок годности");
                ExpDate = Convert.ToInt32(Console.ReadLine());

                DateTime dateForProduction = DateTime.Now.AddDays(-100 + ExpDate);
                currentData = dateForProduction.ToShortDateString();
            }
            else if (readChoise == "2") // Если указать дату производства расчитываем срок годности
            {
                Console.WriteLine("Введите дату производства");
                DateTime dateForProduction = DateTime.Parse(Console.ReadLine());

                TimeSpan dateToExpiration = dateForProduction.AddDays(100) - DateTime.Now;

                currentData = dateForProduction.ToShortDateString();
                ExpDate = Convert.ToInt32(dateToExpiration.TotalDays);
            }
            else
            {
                Console.WriteLine("Неправильный выбор");
                break;
            }

        int boxExpirationDate = ExpDate;
        string boxProductionData = currentData;

        Console.WriteLine("Введите вес коробки");
        int boxWeight = Convert.ToInt32(Console.ReadLine());

        Console.WriteLine("Введите номер палеты для коробки");
        int palletIdToBox = Convert.ToInt32(Console.ReadLine());

        Box newBox = new Box(boxName, boxWidth, boxDepth, boxHeight, boxExpirationDate, boxWeight, boxProductionData);

        SaveBoxToDB(newBox, palletIdToBox); // Сохраняем полученную коробку в БД (атм же изменяются данные паллеты)

        break;

    case "0":
        Environment.Exit(0);
        break;

    default:
        Console.WriteLine("Введите правильное число");
        break;
}
}

// Сортировка групп по сроку годности
static List<Group> GroupNewList(List<Pallet> palletSortNow)
{
    // Console.WriteLine($"всего в базе данных паллет: {palletSortNow.Count}"); --------- ОТЛАДКА

    List<Group> groupNewList = new List<Group>();

    int countGroup = 0;

    Group group = new Group();
    group.palletList = new List<Pallet>();

    // Распределение по группа уже отсротированного списка паллет. 
    for (int i = 0; i < palletSortNow.Count; i++)
    {
        Pallet newGroupPallet = palletSortNow[i];

        if (newGroupPallet.expirationDate == palletSortNow[countGroup].expirationDate)
        {
            group.palletList.Add(newGroupPallet);

            if (i == palletSortNow.Count - 1)
            {
                groupNewList.Add(group);
            }
        }
        else
        {
            groupNewList.Add(group);
            group = new Group();
            group.palletList = new List<Pallet>();

            //Console.WriteLine(i);

            countGroup = i;
            i = i - 1;
        }

    }
    return groupNewList;
}

// Сортировка паллет в группах по весу
static List<Group> GroupListBySortWeight(List<Group> listGroupSort)
{

    List<Group> groupListBySortWeight = new List<Group>();

    int groupCount = 0;
    int groupCountSort = 0;

    int gCont = 0;
    Pallet tempPallet;

    int a = 0;
    int b = 0;

    // Цикл просматривает все группы по очереди.
    while (groupCount < listGroupSort.Count)
    {
        // Console.WriteLine($"всего групп {listGroupSort.Count}"); -------- ОТЛАДКА
        Group newGroupSort = new Group();
        newGroupSort.palletList = new List<Pallet>();
        gCont = 0;
        groupCountSort = 0;

        // Цикл просматривает все паллеты по очереди. Начиная с первой.
        while (gCont < listGroupSort[groupCount].palletList.Count)
        {
            Console.WriteLine($"цикл {gCont}");

            // Цикл просматривает все паллеты по очереди начиная с элемента последующего для сортировки.
            while (groupCountSort < listGroupSort[groupCount].palletList.Count)
            {

                // Console.WriteLine($"чек {groupCountSort} {listGroupSort[groupCount].palletList[groupCountSort].Weight} меньше? {listGroupSort[groupCount].palletList[gCont].Weight} "); -------- ОТЛАДКА ЦИКЛА
                if (listGroupSort[groupCount].palletList[groupCountSort].Weight <= listGroupSort[groupCount].palletList[gCont].Weight)
                {
                    tempPallet = listGroupSort[groupCount].palletList[gCont];
                    listGroupSort[groupCount].palletList[gCont] = listGroupSort[groupCount].palletList[groupCountSort];
                    listGroupSort[groupCount].palletList[groupCountSort] = tempPallet;

                    a = groupCount;
                    b = gCont;

                    // Console.WriteLine($"найдена меньшая паллета {groupCountSort}"); ------------ ОТЛАДКА ЦИКЛА
                }

                groupCountSort++;
            }

            //Console.WriteLine("Добавляю паллету"); --------- ОТЛАДКА ЦИКЛА

            // Добавление отсортированной паллеты по обьему из изначального списка паллет
            newGroupSort.palletList.Add(listGroupSort[a].palletList[b]);

            gCont++;
            groupCountSort = gCont;
        }

        // Добавление Новой группы в конце ее цикла сортировки
        groupListBySortWeight.Add(newGroupSort);
        groupCount++;
    }

    // Второй файл БД для отладки сортировки
    string serializedBox = JsonConvert.SerializeObject(groupListBySortWeight);
    File.WriteAllText("DBs.json", serializedBox);

    return groupListBySortWeight;

}

// Начало обработки баззы данных для отображения
static void sortDB(List<Pallet> DBPalletList) // Получает прочитанную БД
{
    // Проверка БД
    if (DBPalletList == null)
    {
        Console.WriteLine("База данных не сформированна");
        Console.ReadLine();
        Environment.Exit(0);

    }
    int sortIndex = 0;
    int palletCount = DBPalletList.Count;

    List<Pallet> listPalletToSort = DBPalletList; // Копия БД для сортировки, копия отправится в следующие циклы
    while (sortIndex < palletCount) // Проверяет все паллеты
    {

        for (int i = sortIndex; i < palletCount; i++) // Сортирует паллеты по сроку годности
        {
            Pallet palletSortIndexIn = listPalletToSort[i];
            if (palletSortIndexIn.expirationDate < DBPalletList[sortIndex].expirationDate)
            {
                listPalletToSort[sortIndex] = listPalletToSort[i];
            }
        }
        sortIndex++;
    }

    List<Group> sortingGroups = GroupNewList(listPalletToSort); // сортировка групп по сроку годности
    sortingGroups = GroupListBySortWeight(sortingGroups); // Сортировка паллет внутри групп по весу

    // Написание отсортированного результата
    int count = 0;
    while (count < sortingGroups.Count)
    {
        Console.WriteLine($"\n Группа:  {count + 1}");
        for (int i = 0; i < sortingGroups[count].palletList.Count; i++)
        {
            Pallet palletIndexWrite = sortingGroups[count].palletList[i];
            Console.WriteLine($"\t Id Паллеты:  {palletIndexWrite.Id} \t Ширина паллеты: {palletIndexWrite.Width} см \t Глубина паллеты: {palletIndexWrite.Depth} см \t Объем паллеты: {palletIndexWrite.Size} \t вес паллеты: {palletIndexWrite.Weight} кг \t Срок годности паллеты: {palletIndexWrite.expirationDate} д. ");

            if (palletIndexWrite.boxList != null)
            {
                int countBox = 0;
                while (countBox < palletIndexWrite.boxList.Count)
                {

                    Box newCountBox = palletIndexWrite.boxList[countBox];
                    Console.WriteLine($"\t\t\t Название коробки: {newCountBox.Name} \t Ширина коробки: {newCountBox.Width} см \t Глубина коробки: {newCountBox.Depth} см \t Высота коробки: {newCountBox.Height} см \t Срок годности коробки: {newCountBox.expirationDate} д. \t Дата производства коробки: {newCountBox.productionDate} \t вес коробки: {newCountBox.Weight} кг \t Объем коробки: {newCountBox.Size}");

                    countBox++;
                }
                Console.WriteLine("");
            }
            // Console.WriteLine($"ID паллеты: {idPalletSort} \t Ширина паллеты: {widthPalletSort} см \t Глубина паллеты: {depthPalletSort} см \t вес паллеты: {weightPalletSort} кг \t Срок годности паллеты: {eDatePalletSort} дней \t Объем паллеты: {sizePalletSort}");
        }
        count++;
    }

}


static List<Pallet> ReadAllDB()
{
    string json = File.ReadAllText("DB.json");

    List<Pallet> currentPallet = JsonConvert.DeserializeObject<List<Pallet>>(json);

    return currentPallet;
}

// Сохранение коробки в БД
static void SaveBoxToDB(Box box, int idPallet)
{
    List<Pallet> palletListToBox = ReadAllDB();

    // Проверка наличияч БД
    if (palletListToBox == null)
    {
        Console.WriteLine("База данных не сформированна");
    }
    else
    {
        // Поиск паллеты по ID ------------ ID ПАЛЛЕТ НЕ ОТЛАЖЕННО!
        Pallet palletToBox = palletListToBox.Find(pallet => pallet.Id == idPallet);
        int indexTargetBox = palletListToBox.FindIndex(pallet => pallet.Id == idPallet);

        if (BoxChecking(box, palletToBox) == false)
        {
            Console.WriteLine("Паллета слишком мала для коробки");
            return;
        }

        if (palletToBox == null)
        {
            Console.WriteLine("ID паллеты не найден");
            return;
        }
        else //Продолжение 
        {
            // Расчет обьема коробки
            box.Size = box.Width * box.Height * box.Depth;

            // Создание нового списка коробок если у паллеты их не было
            if (palletToBox.boxList == null)
            {
                palletToBox.boxList = new List<Box>();
                palletToBox.boxList.Add(box);

                // Расчет данных о паллете после добавление коробки
                palletToBox.Weight = palletToBox.Weight + box.Weight;
                palletToBox.expirationDate = box.expirationDate;
                palletToBox.Size = box.Size;
            }
            else // Добавление коробок если они уже были
            {
                palletToBox.boxList.Add(box);

                // Расчет данных о паллете после добавление коробки
                palletToBox.Weight = palletToBox.Weight + box.Weight;

                // Расчет минимального срока годности среди всех коробок в паллете
                for (int i = 0; i < palletToBox.boxList.Count; i++)
                {
                    if (palletToBox.boxList[i].expirationDate < palletToBox.expirationDate)
                    {
                        palletToBox.expirationDate = palletToBox.boxList[i].expirationDate;
                    }

                    palletToBox.Size = palletToBox.Size + palletToBox.boxList[i].Size;
                }
            }

            palletListToBox[indexTargetBox] = palletToBox;
        }

    }
    // Сохранение в БД
    string serializedBox = JsonConvert.SerializeObject(palletListToBox);
    File.WriteAllText("DB.json", serializedBox);

}

// Првоерка на доступный обьем паллеты для коробки
static bool BoxChecking(Box boxCheck, Pallet palletToBoxCheck)
{

    if (boxCheck.Width > palletToBoxCheck.Width)
    {
        return false;
    }

    if (boxCheck.Depth > palletToBoxCheck.Depth)
    {
        return false;
    }

    return true;

}

// Сохраняет паллеты в базу данных
static void SavePalletToDB(Pallet pallet)
{

    //Console.WriteLine(pallet.Width); ----- ОТЛАДКА

    List<Pallet> palletList = ReadAllDB();

    // Если БД пустая создаем элементы, иначе добавляем паллету в конец
    if (palletList == null)
    {
        palletList = new List<Pallet>();
        pallet.Id = 1;
        palletList.Add(pallet);
    }
    else
    {
        // Если в бд есть элементы, то необходимо найти
        // айди последнего элемента.
        int palletCount = palletList.Count;
        var lastPallet = palletList[palletCount - 1];
        pallet.Id = lastPallet.Id + 1;

        palletList.Add(pallet);

    }
    // Сохранение БД
    string serializedPallet = JsonConvert.SerializeObject(palletList);
    File.WriteAllText("DB.json", serializedPallet);
}



// Класс паллет
class Pallet
{
    // Переменные паллет: номер палеты, ширина, глубина, вес
    public int Id { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }

    public int expirationDate;
    public int Weight = 30;
    public int Size = 0;
    // public int Weight { get; set; }

    public List<Box> boxList { get; set; }

    public Pallet(int width, int depth)
    {
        Width = width;
        Depth = depth;
    }
    
    
    // Коробки вложенны в палеты
    //Box[] boxName;
}

// Класс коробок
public class Box
{
    // Переменные коробок: номер, ширина, глубина, высота, срок годности, вес
    public string Name { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
    public int Height { get; set; }
    public int expirationDate { get; set; }
    public int Weight { get; set; }
    public int Size = 0;

    // Дата изготовления
    public string productionDate { get; set; }

    public Box(string name, int width, int depth, int height, int eDate, int weight, string prodDate)
    {
        Name = name;
        Width = width;
        Depth = depth;
        Height = height;
        expirationDate = eDate;
        Weight = weight;
        productionDate = prodDate;
    }

}

// Группа для сортировки паллет
class Group
{
    public int Id { get; set; }
    public List<Pallet> palletList { get; set; }
}


    


/* --------------- ОТЛАДКА
        
        Pallet palletSortIndex = DBPalletList[sortIndex];

        string idPalletSort = Convert.ToString(palletSortIndex.Id);
        string widthPalletSort = Convert.ToString(palletSortIndex.Width);
        string depthPalletSort = Convert.ToString(palletSortIndex.Depth);
        string weightPalletSort = Convert.ToString(palletSortIndex.Weight);
        string eDatePalletSort = Convert.ToString(palletSortIndex.expirationDate);
        string sizePalletSort = Convert.ToString(palletSortIndex.Size);

        List<Box> boxPalletSort = palletSortIndex.boxList;

        Console.WriteLine($"ID паллеты: {idPalletSort} \t Ширина паллеты: {widthPalletSort} см \t Глубина паллеты: {depthPalletSort} см \t вес паллеты: {weightPalletSort} кг \t Срок годности паллеты: {eDatePalletSort} дней \t Объем паллеты: {sizePalletSort}");

        if (boxPalletSort == null)
        {
            Console.WriteLine("\t\t\t Коробок в паллете нет\n");
        }
        else
        {
            int sortIndexBox = 0;
            int boxCount = boxPalletSort.Count;

            while (sortIndexBox < boxCount)
            {

                Box boxSortIndex = boxPalletSort[sortIndexBox];

                string nameBoxSort = Convert.ToString(boxSortIndex.Name);
                string widthBoxSort = Convert.ToString(boxSortIndex.Width);
                string depthBoxSort = Convert.ToString(boxSortIndex.Depth);
                string heightBoxSort = Convert.ToString(boxSortIndex.Height);
                string edateBoxSort = Convert.ToString(boxSortIndex.expirationDate);
                string weightBoxSort = Convert.ToString(boxSortIndex.Weight);
                string sizeBoxSort = Convert.ToString(boxSortIndex.Size);
                string productionDBoxSort = boxSortIndex.productionDate;
                Console.WriteLine($"\t\t\t Название коробки: {nameBoxSort} \t Ширина коробки: {widthBoxSort} см \t Глубина коробки: {depthBoxSort} см \t Высота коробки: {heightBoxSort} см \t Срок годности коробки: {edateBoxSort} дней \t Дата производства коробки: {productionDBoxSort} \t вес коробки: {weightBoxSort} кг \t Объем коробки: {sizeBoxSort}");
                sortIndexBox++;
            }
            Console.WriteLine("\n");

        }

        */