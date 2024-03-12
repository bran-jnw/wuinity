namespace WUInity.Visualization
{
	public static class FuelModelColors
	{
		private static WUInityColor ERROR_COLOR = new WUInityColor(1.0f, 0, 1.0f);
		public static WUInityColor GetFuelColor(int fuelNumber)
		{
			if (fuelNumber > 0 && fuelNumber <= colors.Length)
			{
				WUInityColor c = colors[fuelNumber - 1];
				return c;
			}
			return ERROR_COLOR;
		}

        private static readonly WUInityColor AbsoluteZero = new WUInityColor(0, 72, 186, 255);
        private static readonly WUInityColor Acajou = new WUInityColor(76, 47, 39, 255);
        private static readonly WUInityColor AcidGreen = new WUInityColor(176, 191, 26, 255);
        private static readonly WUInityColor Aero = new WUInityColor(124, 185, 232, 255);
        private static readonly WUInityColor AeroBlue = new WUInityColor(201, 255, 229, 255);
        private static readonly WUInityColor AfricanViolet = new WUInityColor(178, 132, 190, 255);
        private static readonly WUInityColor AirForceBlueRAF = new WUInityColor(93, 138, 168, 255);
        private static readonly WUInityColor AirForceBlueUSAF = new WUInityColor(0, 48, 143, 255);
        private static readonly WUInityColor AirSuperiorityBlue = new WUInityColor(114, 160, 193, 255);
        private static readonly WUInityColor AlabamaCrimson = new WUInityColor(175, 48, 42, 255);
        private static readonly WUInityColor Alabaster = new WUInityColor(242, 240, 230, 255);
        private static readonly WUInityColor AliceBlue = new WUInityColor(240, 248, 255, 255);
        private static readonly WUInityColor AlienArmpit = new WUInityColor(132, 222, 2, 255);
        private static readonly WUInityColor AlizarinCrimson = new WUInityColor(227, 38, 54, 255);
        private static readonly WUInityColor AlloyOrange = new WUInityColor(196, 98, 16, 255);
        private static readonly WUInityColor Almond = new WUInityColor(239, 222, 205, 255);
        private static readonly WUInityColor Amaranth = new WUInityColor(229, 43, 80, 255);
        private static readonly WUInityColor AmaranthDeepPurple = new WUInityColor(159, 43, 104, 255);
        private static readonly WUInityColor AmaranthPink = new WUInityColor(241, 156, 187, 255);
        private static readonly WUInityColor AmaranthPurple = new WUInityColor(171, 39, 79, 255);
        private static readonly WUInityColor AmaranthRed = new WUInityColor(211, 33, 45, 255);
        private static readonly WUInityColor Amazon = new WUInityColor(59, 122, 87, 255);
        private static readonly WUInityColor Amazonite = new WUInityColor(0, 196, 176, 255);
        private static readonly WUInityColor Amber = new WUInityColor(255, 191, 0, 255);
        private static readonly WUInityColor AmberSAEECE = new WUInityColor(255, 126, 0, 255);
        private static readonly WUInityColor AmericanBlue = new WUInityColor(59, 59, 109, 255);
        private static readonly WUInityColor AmericanBrown = new WUInityColor(128, 64, 64, 255);
        private static readonly WUInityColor AmericanGold = new WUInityColor(211, 175, 55, 255);
        private static readonly WUInityColor AmericanGreen = new WUInityColor(52, 179, 52, 255);
        private static readonly WUInityColor AmericanOrange = new WUInityColor(255, 139, 0, 255);
        private static readonly WUInityColor AmericanPink = new WUInityColor(255, 152, 153, 255);
        private static readonly WUInityColor AmericanPurple = new WUInityColor(67, 28, 83, 255);
        private static readonly WUInityColor AmericanRed = new WUInityColor(179, 33, 52, 255);
        private static readonly WUInityColor AmericanRose = new WUInityColor(255, 3, 62, 255);
        private static readonly WUInityColor AmericanSilver = new WUInityColor(207, 207, 207, 255);
        private static readonly WUInityColor AmericanViolet = new WUInityColor(85, 27, 140, 255);
        private static readonly WUInityColor AmericanYellow = new WUInityColor(242, 180, 0, 255);
        private static readonly WUInityColor Amethyst = new WUInityColor(153, 102, 204, 255);
        private static readonly WUInityColor AndroidGreen = new WUInityColor(164, 198, 57, 255);
        private static readonly WUInityColor AntiFlashWhite = new WUInityColor(242, 243, 244, 255);
        private static readonly WUInityColor AntiqueBrass = new WUInityColor(205, 149, 117, 255);
        private static readonly WUInityColor AntiqueBronze = new WUInityColor(102, 93, 30, 255);
        private static readonly WUInityColor AntiqueFuchsia = new WUInityColor(145, 92, 131, 255);
        private static readonly WUInityColor AntiqueRuby = new WUInityColor(132, 27, 45, 255);
        private static readonly WUInityColor AntiqueWhite = new WUInityColor(250, 235, 215, 255);
        private static readonly WUInityColor AoEnglish = new WUInityColor(0, 128, 0, 255);
        private static readonly WUInityColor Apple = new WUInityColor(102, 180, 71, 255);
        private static readonly WUInityColor AppleGreen = new WUInityColor(141, 182, 0, 255);
        private static readonly WUInityColor Apricot = new WUInityColor(251, 206, 177, 255);
        private static readonly WUInityColor Aqua = new WUInityColor(0, 255, 255, 255);
        private static readonly WUInityColor Aquamarine = new WUInityColor(127, 255, 212, 255);
        private static readonly WUInityColor ArcticLime = new WUInityColor(208, 255, 20, 255);
        private static readonly WUInityColor ArmyGreen = new WUInityColor(75, 83, 32, 255);
        private static readonly WUInityColor Arsenic = new WUInityColor(59, 68, 75, 255);
        private static readonly WUInityColor Artichoke = new WUInityColor(143, 151, 121, 255);
        private static readonly WUInityColor ArylideYellow = new WUInityColor(233, 214, 107, 255);
        private static readonly WUInityColor AshGray = new WUInityColor(178, 190, 181, 255);
        private static readonly WUInityColor Asparagus = new WUInityColor(135, 169, 107, 255);
        private static readonly WUInityColor AteneoBlue = new WUInityColor(0, 58, 108, 255);
        private static readonly WUInityColor AtomicTangerine = new WUInityColor(255, 153, 102, 255);
        private static readonly WUInityColor Auburn = new WUInityColor(165, 42, 42, 255);
        private static readonly WUInityColor Aureolin = new WUInityColor(253, 238, 0, 255);
        private static readonly WUInityColor Aurometalsaurus = new WUInityColor(110, 127, 128, 255);
        private static readonly WUInityColor Avocado = new WUInityColor(86, 130, 3, 255);
        private static readonly WUInityColor Awesome = new WUInityColor(255, 32, 82, 255);
        private static readonly WUInityColor Axolotl = new WUInityColor(99, 119, 91, 255);
        private static readonly WUInityColor AztecGold = new WUInityColor(195, 153, 83, 255);
        private static readonly WUInityColor Azure = new WUInityColor(0, 127, 255, 255);
        private static readonly WUInityColor AzureWebColor = new WUInityColor(240, 255, 255, 255);
        private static readonly WUInityColor AzureishWhite = new WUInityColor(219, 233, 244, 255);
        private static readonly WUInityColor BabyBlue = new WUInityColor(137, 207, 240, 255);
        private static readonly WUInityColor BabyBlueEyes = new WUInityColor(161, 202, 241, 255);
        private static readonly WUInityColor BabyPink = new WUInityColor(244, 194, 194, 255);
        private static readonly WUInityColor BabyPowder = new WUInityColor(254, 254, 250, 255);
        private static readonly WUInityColor BakerMillerPink = new WUInityColor(255, 145, 175, 255);
        private static readonly WUInityColor BallBlue = new WUInityColor(33, 171, 205, 255);
        private static readonly WUInityColor BananaMania = new WUInityColor(250, 231, 181, 255);
        private static readonly WUInityColor BananaYellow = new WUInityColor(255, 225, 53, 255);
        private static readonly WUInityColor BangladeshGreen = new WUInityColor(0, 106, 78, 255);
        private static readonly WUInityColor BarbiePink = new WUInityColor(224, 33, 138, 255);
        private static readonly WUInityColor BarnRed = new WUInityColor(124, 10, 2, 255);
        private static readonly WUInityColor BatteryChargedBlue = new WUInityColor(29, 172, 214, 255);
        private static readonly WUInityColor BattleshipGrey = new WUInityColor(132, 132, 130, 255);
        private static readonly WUInityColor Bazaar = new WUInityColor(152, 119, 123, 255);
        private static readonly WUInityColor BeauBlue = new WUInityColor(188, 212, 230, 255);
        private static readonly WUInityColor Beaver = new WUInityColor(159, 129, 112, 255);
        private static readonly WUInityColor Begonia = new WUInityColor(250, 110, 121, 255);
        private static readonly WUInityColor Beige = new WUInityColor(245, 245, 220, 255);
        private static readonly WUInityColor BdazzledBlue = new WUInityColor(46, 88, 148, 255);
        private static readonly WUInityColor BigDipORuby = new WUInityColor(156, 37, 66, 255);
        private static readonly WUInityColor BigFootFeet = new WUInityColor(232, 142, 90, 255);
        private static readonly WUInityColor Bisque = new WUInityColor(255, 228, 196, 255);
        private static readonly WUInityColor Bistre = new WUInityColor(61, 43, 31, 255);
        private static readonly WUInityColor BistreBrown = new WUInityColor(150, 113, 23, 255);
        private static readonly WUInityColor BitterLemon = new WUInityColor(202, 224, 13, 255);
        private static readonly WUInityColor BitterLime = new WUInityColor(191, 255, 0, 255);
        private static readonly WUInityColor Bittersweet = new WUInityColor(254, 111, 94, 255);
        private static readonly WUInityColor BittersweetShimmer = new WUInityColor(191, 79, 81, 255);
        private static readonly WUInityColor Black = new WUInityColor(0, 0, 0, 255);
        private static readonly WUInityColor BlackBean = new WUInityColor(61, 12, 2, 255);
        private static readonly WUInityColor BlackChocolate = new WUInityColor(27, 24, 17, 255);
        private static readonly WUInityColor BlackCoffee = new WUInityColor(59, 47, 47, 255);
        private static readonly WUInityColor BlackCoral = new WUInityColor(84, 98, 111, 255);
        private static readonly WUInityColor BlackLeatherJacket = new WUInityColor(37, 53, 41, 255);
        private static readonly WUInityColor BlackOlive = new WUInityColor(59, 60, 54, 255);
        private static readonly WUInityColor Blackberry = new WUInityColor(143, 89, 115, 255);
        private static readonly WUInityColor BlackShadows = new WUInityColor(191, 175, 178, 255);
        private static readonly WUInityColor BlanchedAlmond = new WUInityColor(255, 235, 205, 255);
        private static readonly WUInityColor BlastOffBronze = new WUInityColor(165, 113, 100, 255);
        private static readonly WUInityColor BleuDeFrance = new WUInityColor(49, 140, 231, 255);
        private static readonly WUInityColor BlizzardBlue = new WUInityColor(172, 229, 238, 255);
        private static readonly WUInityColor Blond = new WUInityColor(250, 240, 190, 255);
        private static readonly WUInityColor BloodOrange = new WUInityColor(210, 0, 27, 255);
        private static readonly WUInityColor BloodRed = new WUInityColor(102, 0, 0, 255);
        private static readonly WUInityColor Blue = new WUInityColor(0, 0, 255, 255);
        private static readonly WUInityColor BlueCrayola = new WUInityColor(31, 117, 254, 255);
        private static readonly WUInityColor BlueMunsell = new WUInityColor(0, 147, 175, 255);
        private static readonly WUInityColor BlueNCS = new WUInityColor(0, 135, 189, 255);
        private static readonly WUInityColor BluePantone = new WUInityColor(0, 24, 168, 255);
        private static readonly WUInityColor BluePigment = new WUInityColor(51, 51, 153, 255);
        private static readonly WUInityColor BlueRYB = new WUInityColor(2, 71, 254, 255);
        private static readonly WUInityColor BlueBell = new WUInityColor(162, 162, 208, 255);
        private static readonly WUInityColor BlueBolt = new WUInityColor(0, 185, 251, 255);
        private static readonly WUInityColor BlueGray = new WUInityColor(102, 153, 204, 255);
        private static readonly WUInityColor BlueGreen = new WUInityColor(13, 152, 186, 255);
        private static readonly WUInityColor BlueJeans = new WUInityColor(93, 173, 236, 255);
        private static readonly WUInityColor BlueMagentaViolet = new WUInityColor(85, 53, 146, 255);
        private static readonly WUInityColor BlueSapphire = new WUInityColor(18, 97, 128, 255);
        private static readonly WUInityColor BlueViolet = new WUInityColor(138, 43, 226, 255);
        private static readonly WUInityColor BlueYonder = new WUInityColor(80, 114, 167, 255);
        private static readonly WUInityColor Blueberry = new WUInityColor(79, 134, 247, 255);
        private static readonly WUInityColor Bluebonnet = new WUInityColor(28, 28, 240, 255);
        private static readonly WUInityColor Blush = new WUInityColor(222, 93, 131, 255);
        private static readonly WUInityColor Bole = new WUInityColor(121, 68, 59, 255);
        private static readonly WUInityColor BondiBlue = new WUInityColor(0, 149, 182, 255);
        private static readonly WUInityColor Bone = new WUInityColor(227, 218, 201, 255);
        private static readonly WUInityColor BoogerBuster = new WUInityColor(221, 226, 106, 255);
        private static readonly WUInityColor BostonUniversityRed = new WUInityColor(204, 0, 0, 255);
        private static readonly WUInityColor Boysenberry = new WUInityColor(135, 50, 96, 255);
        private static readonly WUInityColor BrandeisBlue = new WUInityColor(0, 112, 255, 255);
        private static readonly WUInityColor Brass = new WUInityColor(181, 166, 66, 255);
        private static readonly WUInityColor BrickRed = new WUInityColor(203, 65, 84, 255);
        private static readonly WUInityColor BrightGray = new WUInityColor(235, 236, 240, 255);
        private static readonly WUInityColor BrightGreen = new WUInityColor(102, 255, 0, 255);
        private static readonly WUInityColor BrightLavender = new WUInityColor(191, 148, 228, 255);
        private static readonly WUInityColor BrightLilac = new WUInityColor(216, 145, 239, 255);
        private static readonly WUInityColor BrightMaroon = new WUInityColor(195, 33, 72, 255);
        private static readonly WUInityColor BrightNavyBlue = new WUInityColor(25, 116, 210, 255);
        private static readonly WUInityColor BrightPink = new WUInityColor(255, 0, 127, 255);
        private static readonly WUInityColor BrightTurquoise = new WUInityColor(8, 232, 222, 255);
        private static readonly WUInityColor BrightUbe = new WUInityColor(209, 159, 232, 255);
        private static readonly WUInityColor BrightYellowCrayola = new WUInityColor(255, 170, 29, 255);
        private static readonly WUInityColor BrilliantAzure = new WUInityColor(51, 153, 255, 255);
        private static readonly WUInityColor BrilliantLavender = new WUInityColor(244, 187, 255, 255);
        private static readonly WUInityColor BrilliantRose = new WUInityColor(255, 85, 163, 255);
        private static readonly WUInityColor BrinkPink = new WUInityColor(251, 96, 127, 255);
        private static readonly WUInityColor BritishRacingGreen = new WUInityColor(0, 66, 37, 255);
        private static readonly WUInityColor Bronze = new WUInityColor(136, 84, 11, 255);
        private static readonly WUInityColor Bronze2 = new WUInityColor(205, 127, 50, 255);
        private static readonly WUInityColor BronzeMetallic = new WUInityColor(176, 140, 86, 255);
        private static readonly WUInityColor BronzeYellow = new WUInityColor(115, 112, 0, 255);
        private static readonly WUInityColor Brown = new WUInityColor(153, 51, 0, 255);
        private static readonly WUInityColor BrownCrayola = new WUInityColor(175, 89, 62, 255);
        private static readonly WUInityColor BrownTraditional = new WUInityColor(150, 75, 0, 255);
        private static readonly WUInityColor BrownNose = new WUInityColor(107, 68, 35, 255);
        private static readonly WUInityColor BrownSugar = new WUInityColor(175, 110, 77, 255);
        private static readonly WUInityColor BrownChocolate = new WUInityColor(95, 25, 51, 255);
        private static readonly WUInityColor BrownCoffee = new WUInityColor(74, 44, 42, 255);
        private static readonly WUInityColor BrownYellow = new WUInityColor(204, 153, 102, 255);
        private static readonly WUInityColor BrunswickGreen = new WUInityColor(27, 77, 62, 255);
        private static readonly WUInityColor BubbleGum = new WUInityColor(255, 193, 204, 255);
        private static readonly WUInityColor Bubbles = new WUInityColor(231, 254, 255, 255);
        private static readonly WUInityColor BudGreen = new WUInityColor(123, 182, 97, 255);
        private static readonly WUInityColor Buff = new WUInityColor(240, 220, 130, 255);
        private static readonly WUInityColor BulgarianRose = new WUInityColor(72, 6, 7, 255);
        private static readonly WUInityColor Burgundy = new WUInityColor(128, 0, 32, 255);
        private static readonly WUInityColor Burlywood = new WUInityColor(222, 184, 135, 255);
        private static readonly WUInityColor BurnishedBrown = new WUInityColor(161, 122, 116, 255);
        private static readonly WUInityColor BurntOrange = new WUInityColor(204, 85, 0, 255);
        private static readonly WUInityColor BurntSienna = new WUInityColor(233, 116, 81, 255);
        private static readonly WUInityColor BurntUmber = new WUInityColor(138, 51, 36, 255);
        private static readonly WUInityColor ButtonBlue = new WUInityColor(36, 160, 237, 255);
        private static readonly WUInityColor Byzantine = new WUInityColor(189, 51, 164, 255);
        private static readonly WUInityColor Byzantium = new WUInityColor(112, 41, 99, 255);
        private static readonly WUInityColor Cadet = new WUInityColor(83, 104, 114, 255);
        private static readonly WUInityColor CadetBlue = new WUInityColor(95, 158, 160, 255);
        private static readonly WUInityColor CadetGrey = new WUInityColor(145, 163, 176, 255);
        private static readonly WUInityColor CadmiumBlue = new WUInityColor(10, 17, 146, 255);
        private static readonly WUInityColor CadmiumGreen = new WUInityColor(0, 107, 60, 255);
        private static readonly WUInityColor CadmiumOrange = new WUInityColor(237, 135, 45, 255);
        private static readonly WUInityColor CadmiumPurple = new WUInityColor(182, 12, 38, 255);
        private static readonly WUInityColor CadmiumRed = new WUInityColor(227, 0, 34, 255);
        private static readonly WUInityColor CadmiumYellow = new WUInityColor(255, 246, 0, 255);
        private static readonly WUInityColor CadmiumViolet = new WUInityColor(127, 62, 152, 255);
        private static readonly WUInityColor CaféAuLait = new WUInityColor(166, 123, 91, 255);
        private static readonly WUInityColor CaféNoir = new WUInityColor(75, 54, 33, 255);
        private static readonly WUInityColor CalPolyPomonaGreen = new WUInityColor(30, 77, 43, 255);
        private static readonly WUInityColor Calamansi = new WUInityColor(252, 255, 164, 255);
        private static readonly WUInityColor CambridgeBlue = new WUInityColor(163, 193, 173, 255);
        private static readonly WUInityColor Camel = new WUInityColor(193, 154, 107, 255);
        private static readonly WUInityColor CameoPink = new WUInityColor(239, 187, 204, 255);
        private static readonly WUInityColor CamouflageGreen = new WUInityColor(120, 134, 107, 255);
        private static readonly WUInityColor Canary = new WUInityColor(255, 255, 153, 255);
        private static readonly WUInityColor CanaryYellow = new WUInityColor(255, 239, 0, 255);
        private static readonly WUInityColor CandyAppleRed = new WUInityColor(255, 8, 0, 255);
        private static readonly WUInityColor CandyPink = new WUInityColor(228, 113, 122, 255);
        private static readonly WUInityColor Capri = new WUInityColor(0, 191, 255, 255);
        private static readonly WUInityColor CaputMortuum = new WUInityColor(89, 39, 32, 255);
        private static readonly WUInityColor Caramel = new WUInityColor(255, 213, 154, 255);
        private static readonly WUInityColor Cardinal = new WUInityColor(196, 30, 58, 255);
        private static readonly WUInityColor CaribbeanGreen = new WUInityColor(0, 204, 153, 255);
        private static readonly WUInityColor Carmine = new WUInityColor(150, 0, 24, 255);
        private static readonly WUInityColor CarmineMP = new WUInityColor(215, 0, 64, 255);
        private static readonly WUInityColor CarminePink = new WUInityColor(235, 76, 66, 255);
        private static readonly WUInityColor CarmineRed = new WUInityColor(255, 0, 56, 255);
        private static readonly WUInityColor CarnationPink = new WUInityColor(255, 166, 201, 255);
        private static readonly WUInityColor Carnelian = new WUInityColor(179, 27, 27, 255);
        private static readonly WUInityColor CarolinaBlue = new WUInityColor(86, 160, 211, 255);
        private static readonly WUInityColor CarrotOrange = new WUInityColor(237, 145, 33, 255);
        private static readonly WUInityColor CastletonGreen = new WUInityColor(0, 86, 63, 255);
        private static readonly WUInityColor CatalinaBlue = new WUInityColor(6, 42, 120, 255);
        private static readonly WUInityColor Catawba = new WUInityColor(112, 54, 66, 255);
        private static readonly WUInityColor CedarChest = new WUInityColor(201, 90, 73, 255);
        private static readonly WUInityColor Ceil = new WUInityColor(146, 161, 207, 255);
        private static readonly WUInityColor Celadon = new WUInityColor(172, 225, 175, 255);
        private static readonly WUInityColor CeladonBlue = new WUInityColor(0, 123, 167, 255);
        private static readonly WUInityColor CeladonGreen = new WUInityColor(47, 132, 124, 255);
        private static readonly WUInityColor Celeste = new WUInityColor(178, 255, 255, 255);
        private static readonly WUInityColor CelestialBlue = new WUInityColor(73, 151, 208, 255);
        private static readonly WUInityColor Cerise = new WUInityColor(222, 49, 99, 255);
        private static readonly WUInityColor CerisePink = new WUInityColor(236, 59, 131, 255);
        private static readonly WUInityColor CeruleanBlue = new WUInityColor(42, 82, 190, 255);
        private static readonly WUInityColor CeruleanFrost = new WUInityColor(109, 155, 195, 255);
        private static readonly WUInityColor CGBlue = new WUInityColor(0, 122, 165, 255);
        private static readonly WUInityColor CGRed = new WUInityColor(224, 60, 49, 255);
        private static readonly WUInityColor Chamoisee = new WUInityColor(160, 120, 90, 255);
        private static readonly WUInityColor Champagne = new WUInityColor(247, 231, 206, 255);
        private static readonly WUInityColor ChampagnePink = new WUInityColor(241, 221, 207, 255);
        private static readonly WUInityColor Charcoal = new WUInityColor(54, 69, 79, 255);
        private static readonly WUInityColor CharlestonGreen = new WUInityColor(35, 43, 43, 255);
        private static readonly WUInityColor Charm = new WUInityColor(208, 116, 139, 255);
        private static readonly WUInityColor CharmPink = new WUInityColor(230, 143, 172, 255);
        private static readonly WUInityColor ChartreuseTraditional = new WUInityColor(223, 255, 0, 255);
        private static readonly WUInityColor ChartreuseWeb = new WUInityColor(127, 255, 0, 255);
        private static readonly WUInityColor Cheese = new WUInityColor(255, 166, 0, 255);
        private static readonly WUInityColor CherryBlossomPink = new WUInityColor(255, 183, 197, 255);
        private static readonly WUInityColor Chestnut = new WUInityColor(149, 69, 53, 255);
        private static readonly WUInityColor ChinaPink = new WUInityColor(222, 111, 161, 255);
        private static readonly WUInityColor ChinaRose = new WUInityColor(168, 81, 110, 255);
        private static readonly WUInityColor ChineseBlack = new WUInityColor(20, 20, 20, 255);
        private static readonly WUInityColor ChineseBlue = new WUInityColor(54, 81, 148, 255);
        private static readonly WUInityColor ChineseBronze = new WUInityColor(205, 128, 50, 255);
        private static readonly WUInityColor ChineseBrown = new WUInityColor(170, 56, 30, 255);
        private static readonly WUInityColor ChineseGreen = new WUInityColor(208, 219, 97, 255);
        private static readonly WUInityColor ChineseGold = new WUInityColor(204, 153, 0, 255);
        private static readonly WUInityColor ChineseOrange = new WUInityColor(243, 112, 66, 255);
        private static readonly WUInityColor ChinesePink = new WUInityColor(222, 112, 161, 255);
        private static readonly WUInityColor ChinesePurple = new WUInityColor(114, 11, 152, 255);
        private static readonly WUInityColor ChineseRed = new WUInityColor(205, 7, 30, 255);
        private static readonly WUInityColor ChineseSilver = new WUInityColor(204, 204, 204, 255);
        private static readonly WUInityColor ChineseViolet = new WUInityColor(133, 96, 136, 255);
        private static readonly WUInityColor ChineseWhite = new WUInityColor(226, 229, 222, 255);
        private static readonly WUInityColor ChineseYellow = new WUInityColor(255, 178, 0, 255);
        private static readonly WUInityColor ChlorophyllGreen = new WUInityColor(74, 255, 0, 255);
        private static readonly WUInityColor ChocolateKisses = new WUInityColor(60, 20, 33, 255);
        private static readonly WUInityColor ChocolateTraditional = new WUInityColor(123, 63, 0, 255);
        private static readonly WUInityColor ChocolateWeb = new WUInityColor(210, 105, 30, 255);
        private static readonly WUInityColor ChristmasBlue = new WUInityColor(42, 143, 189, 255);
        private static readonly WUInityColor ChristmasBrown = new WUInityColor(93, 43, 44, 255);
        private static readonly WUInityColor ChristmasBrown2 = new WUInityColor(76, 31, 2, 255);
        private static readonly WUInityColor ChristmasGreen = new WUInityColor(60, 141, 13, 255);
        private static readonly WUInityColor ChristmasGreen2 = new WUInityColor(0, 117, 2, 255);
        private static readonly WUInityColor ChristmasGold = new WUInityColor(202, 169, 6, 255);
        private static readonly WUInityColor ChristmasOrange = new WUInityColor(255, 102, 0, 255);
        private static readonly WUInityColor ChristmasOrange2 = new WUInityColor(213, 108, 43, 255);
        private static readonly WUInityColor ChristmasPink = new WUInityColor(255, 204, 203, 255);
        private static readonly WUInityColor ChristmasPink2 = new WUInityColor(227, 66, 133, 255);
        private static readonly WUInityColor ChristmasPurple = new WUInityColor(102, 51, 152, 255);
        private static readonly WUInityColor ChristmasPurple2 = new WUInityColor(77, 8, 77, 255);
        private static readonly WUInityColor ChristmasRed = new WUInityColor(170, 1, 20, 255);
        private static readonly WUInityColor ChristmasRed2 = new WUInityColor(176, 27, 46, 255);
        private static readonly WUInityColor ChristmasSilver = new WUInityColor(225, 223, 224, 255);
        private static readonly WUInityColor ChristmasYellow = new WUInityColor(255, 204, 0, 255);
        private static readonly WUInityColor ChristmasYellow2 = new WUInityColor(254, 242, 0, 255);
        private static readonly WUInityColor ChromeYellow = new WUInityColor(255, 167, 0, 255);
        private static readonly WUInityColor Cinereous = new WUInityColor(152, 129, 123, 255);
        private static readonly WUInityColor Cinnabar = new WUInityColor(227, 66, 52, 255);
        private static readonly WUInityColor CinnamonSatin = new WUInityColor(205, 96, 126, 255);
        private static readonly WUInityColor Citrine = new WUInityColor(228, 208, 10, 255);
        private static readonly WUInityColor CitrineBrown = new WUInityColor(147, 55, 9, 255);
        private static readonly WUInityColor Citron = new WUInityColor(158, 169, 31, 255);
        private static readonly WUInityColor Claret = new WUInityColor(127, 23, 52, 255);
        private static readonly WUInityColor ClassicRose = new WUInityColor(251, 204, 231, 255);
        private static readonly WUInityColor CobaltBlue = new WUInityColor(0, 71, 171, 255);
        private static readonly WUInityColor Coconut = new WUInityColor(150, 90, 62, 255);
        private static readonly WUInityColor Coffee = new WUInityColor(111, 78, 55, 255);
        private static readonly WUInityColor Cola = new WUInityColor(60, 48, 36, 255);
        private static readonly WUInityColor ColumbiaBlue = new WUInityColor(196, 216, 226, 255);
        private static readonly WUInityColor Conditioner = new WUInityColor(255, 255, 204, 255);
        private static readonly WUInityColor CongoPink = new WUInityColor(248, 131, 121, 255);
        private static readonly WUInityColor CoolBlack = new WUInityColor(0, 46, 99, 255);
        private static readonly WUInityColor CoolGrey = new WUInityColor(140, 146, 172, 255);
        private static readonly WUInityColor CookiesAndCream = new WUInityColor(238, 224, 177, 255);
        private static readonly WUInityColor Copper = new WUInityColor(184, 115, 51, 255);
        private static readonly WUInityColor CopperCrayola = new WUInityColor(218, 138, 103, 255);
        private static readonly WUInityColor CopperPenny = new WUInityColor(173, 111, 105, 255);
        private static readonly WUInityColor CopperRed = new WUInityColor(203, 109, 81, 255);
        private static readonly WUInityColor CopperRose = new WUInityColor(153, 102, 102, 255);
        private static readonly WUInityColor Coquelicot = new WUInityColor(255, 56, 0, 255);
        private static readonly WUInityColor Coral = new WUInityColor(255, 127, 80, 255);
        private static readonly WUInityColor CoralRed = new WUInityColor(255, 64, 64, 255);
        private static readonly WUInityColor CoralReef = new WUInityColor(253, 124, 110, 255);
        private static readonly WUInityColor Cordovan = new WUInityColor(137, 63, 69, 255);
        private static readonly WUInityColor Corn = new WUInityColor(251, 236, 93, 255);
        private static readonly WUInityColor CornflowerBlue = new WUInityColor(100, 149, 237, 255);
        private static readonly WUInityColor Cornsilk = new WUInityColor(255, 248, 220, 255);
        private static readonly WUInityColor CosmicCobalt = new WUInityColor(46, 45, 136, 255);
        private static readonly WUInityColor CosmicLatte = new WUInityColor(255, 248, 231, 255);
        private static readonly WUInityColor CoyoteBrown = new WUInityColor(129, 97, 60, 255);
        private static readonly WUInityColor CottonCandy = new WUInityColor(255, 188, 217, 255);
        private static readonly WUInityColor Cream = new WUInityColor(255, 253, 208, 255);
        private static readonly WUInityColor Crimson = new WUInityColor(220, 20, 60, 255);
        private static readonly WUInityColor CrimsonGlory = new WUInityColor(190, 0, 50, 255);
        private static readonly WUInityColor CrimsonRed = new WUInityColor(153, 0, 0, 255);
        private static readonly WUInityColor Cultured = new WUInityColor(245, 245, 245, 255);
        private static readonly WUInityColor CyanAzure = new WUInityColor(78, 130, 180, 255);
        private static readonly WUInityColor CyanBlueAzure = new WUInityColor(70, 130, 191, 255);
        private static readonly WUInityColor CyanCobaltBlue = new WUInityColor(40, 88, 156, 255);
        private static readonly WUInityColor CyanCornflowerBlue = new WUInityColor(24, 139, 194, 255);
        private static readonly WUInityColor CyanProcess = new WUInityColor(0, 183, 235, 255);
        private static readonly WUInityColor CyberGrape = new WUInityColor(88, 66, 124, 255);
        private static readonly WUInityColor CyberYellow = new WUInityColor(255, 211, 0, 255);
        private static readonly WUInityColor Cyclamen = new WUInityColor(245, 111, 161, 255);
        private static readonly WUInityColor Daffodil = new WUInityColor(255, 255, 49, 255);
        private static readonly WUInityColor Dandelion = new WUInityColor(240, 225, 48, 255);
        private static readonly WUInityColor DarkBlue = new WUInityColor(0, 0, 139, 255);
        private static readonly WUInityColor DarkBlueGray = new WUInityColor(102, 102, 153, 255);
        private static readonly WUInityColor DarkBronze = new WUInityColor(128, 74, 0, 255);
        private static readonly WUInityColor DarkBrown = new WUInityColor(101, 67, 33, 255);
        private static readonly WUInityColor DarkBrownTangelo = new WUInityColor(136, 101, 78, 255);
        private static readonly WUInityColor DarkByzantium = new WUInityColor(93, 57, 84, 255);
        private static readonly WUInityColor DarkCandyAppleRed = new WUInityColor(164, 0, 0, 255);
        private static readonly WUInityColor DarkCerulean = new WUInityColor(8, 69, 126, 255);
        private static readonly WUInityColor DarkCharcoal = new WUInityColor(51, 51, 51, 255);
        private static readonly WUInityColor DarkChestnut = new WUInityColor(152, 105, 96, 255);
        private static readonly WUInityColor DarkChocolate = new WUInityColor(73, 2, 6, 255);
        private static readonly WUInityColor DarkChocolateHersheys = new WUInityColor(60, 19, 33, 255);
        private static readonly WUInityColor DarkCornflowerBlue = new WUInityColor(38, 66, 139, 255);
        private static readonly WUInityColor DarkCoral = new WUInityColor(205, 91, 69, 255);
        private static readonly WUInityColor DarkCyan = new WUInityColor(0, 139, 139, 255);
        private static readonly WUInityColor DarkElectricBlue = new WUInityColor(83, 104, 120, 255);
        private static readonly WUInityColor DarkGoldenrod = new WUInityColor(184, 134, 11, 255);
        private static readonly WUInityColor DarkGrayX11 = new WUInityColor(169, 169, 169, 255);
        private static readonly WUInityColor DarkGreen = new WUInityColor(1, 50, 32, 255);
        private static readonly WUInityColor DarkGreenX11 = new WUInityColor(0, 100, 0, 255);
        private static readonly WUInityColor DarkGunmetal = new WUInityColor(31, 38, 42, 255);
        private static readonly WUInityColor DarkImperialBlue = new WUInityColor(0, 65, 106, 255);
        private static readonly WUInityColor DarkImperialBlue2 = new WUInityColor(0, 20, 126, 255);
        private static readonly WUInityColor DarkJungleGreen = new WUInityColor(26, 36, 33, 255);
        private static readonly WUInityColor DarkKhaki = new WUInityColor(189, 183, 107, 255);
        private static readonly WUInityColor DarkLava = new WUInityColor(72, 60, 50, 255);
        private static readonly WUInityColor DarkLavender = new WUInityColor(115, 79, 150, 255);
        private static readonly WUInityColor DarkLemonLime = new WUInityColor(139, 190, 27, 255);
        private static readonly WUInityColor DarkLiver = new WUInityColor(83, 75, 79, 255);
        private static readonly WUInityColor DarkLiverHorses = new WUInityColor(84, 61, 55, 255);
        private static readonly WUInityColor DarkMagenta = new WUInityColor(139, 0, 139, 255);
        private static readonly WUInityColor DarkMidnightBlue = new WUInityColor(0, 51, 102, 255);
        private static readonly WUInityColor DarkMossGreen = new WUInityColor(74, 93, 35, 255);
        private static readonly WUInityColor DarkOliveGreen = new WUInityColor(85, 107, 47, 255);
        private static readonly WUInityColor DarkOrange = new WUInityColor(255, 140, 0, 255);
        private static readonly WUInityColor DarkOrchid = new WUInityColor(153, 50, 204, 255);
        private static readonly WUInityColor DarkPastelBlue = new WUInityColor(119, 158, 203, 255);
        private static readonly WUInityColor DarkPastelGreen = new WUInityColor(3, 192, 60, 255);
        private static readonly WUInityColor DarkPastelPurple = new WUInityColor(150, 111, 214, 255);
        private static readonly WUInityColor DarkPastelRed = new WUInityColor(194, 59, 34, 255);
        private static readonly WUInityColor DarkPink = new WUInityColor(231, 84, 128, 255);
        private static readonly WUInityColor DarkPowderBlue = new WUInityColor(0, 51, 153, 255);
        private static readonly WUInityColor DarkPuce = new WUInityColor(79, 58, 60, 255);
        private static readonly WUInityColor DarkPurple = new WUInityColor(48, 25, 52, 255);
        private static readonly WUInityColor DarkRaspberry = new WUInityColor(135, 38, 87, 255);
        private static readonly WUInityColor DarkRed = new WUInityColor(139, 0, 0, 255);
        private static readonly WUInityColor DarkSalmon = new WUInityColor(233, 150, 122, 255);
        private static readonly WUInityColor DarkScarlet = new WUInityColor(86, 3, 25, 255);
        private static readonly WUInityColor DarkSeaGreen = new WUInityColor(143, 188, 143, 255);
        private static readonly WUInityColor DarkSienna = new WUInityColor(60, 20, 20, 255);
        private static readonly WUInityColor DarkSkyBlue = new WUInityColor(140, 190, 214, 255);
        private static readonly WUInityColor DarkSlateBlue = new WUInityColor(72, 61, 139, 255);
        private static readonly WUInityColor DarkSlateGray = new WUInityColor(47, 79, 79, 255);
        private static readonly WUInityColor DarkSpringGreen = new WUInityColor(23, 114, 69, 255);
        private static readonly WUInityColor DarkTan = new WUInityColor(145, 129, 81, 255);
        private static readonly WUInityColor DarkTangerine = new WUInityColor(255, 168, 18, 255);
        private static readonly WUInityColor DarkTerraCotta = new WUInityColor(204, 78, 92, 255);
        private static readonly WUInityColor DarkTurquoise = new WUInityColor(0, 206, 209, 255);
        private static readonly WUInityColor DarkVanilla = new WUInityColor(209, 190, 168, 255);
        private static readonly WUInityColor DarkViolet = new WUInityColor(148, 0, 211, 255);
        private static readonly WUInityColor DarkYellow = new WUInityColor(155, 135, 12, 255);
        private static readonly WUInityColor DartmouthGreen = new WUInityColor(0, 112, 60, 255);
        private static readonly WUInityColor DavysGrey = new WUInityColor(85, 85, 85, 255);
        private static readonly WUInityColor DebianRed = new WUInityColor(215, 10, 83, 255);
        private static readonly WUInityColor DeepAmethyst = new WUInityColor(156, 138, 164, 255);
        private static readonly WUInityColor DeepAquamarine = new WUInityColor(64, 130, 109, 255);
        private static readonly WUInityColor DeepCarmine = new WUInityColor(169, 32, 62, 255);
        private static readonly WUInityColor DeepCarminePink = new WUInityColor(239, 48, 56, 255);
        private static readonly WUInityColor DeepCarrotOrange = new WUInityColor(233, 105, 44, 255);
        private static readonly WUInityColor DeepCerise = new WUInityColor(218, 50, 135, 255);
        private static readonly WUInityColor DeepChampagne = new WUInityColor(250, 214, 165, 255);
        private static readonly WUInityColor DeepChestnut = new WUInityColor(185, 78, 72, 255);
        private static readonly WUInityColor DeepCoffee = new WUInityColor(112, 66, 65, 255);
        private static readonly WUInityColor DeepFuchsia = new WUInityColor(193, 84, 193, 255);
        private static readonly WUInityColor DeepGreen = new WUInityColor(5, 102, 8, 255);
        private static readonly WUInityColor DeepGreenCyanTurquoise = new WUInityColor(14, 124, 97, 255);
        private static readonly WUInityColor DeepJungleGreen = new WUInityColor(0, 75, 73, 255);
        private static readonly WUInityColor DeepKoamaru = new WUInityColor(51, 51, 102, 255);
        private static readonly WUInityColor DeepLemon = new WUInityColor(245, 199, 26, 255);
        private static readonly WUInityColor DeepLilac = new WUInityColor(153, 85, 187, 255);
        private static readonly WUInityColor DeepMagenta = new WUInityColor(204, 0, 204, 255);
        private static readonly WUInityColor DeepMaroon = new WUInityColor(130, 0, 0, 255);
        private static readonly WUInityColor DeepMauve = new WUInityColor(212, 115, 212, 255);
        private static readonly WUInityColor DeepMossGreen = new WUInityColor(53, 94, 59, 255);
        private static readonly WUInityColor DeepPeach = new WUInityColor(255, 203, 164, 255);
        private static readonly WUInityColor DeepPink = new WUInityColor(255, 20, 147, 255);
        private static readonly WUInityColor DeepPuce = new WUInityColor(169, 92, 104, 255);
        private static readonly WUInityColor DeepRed = new WUInityColor(133, 1, 1, 255);
        private static readonly WUInityColor DeepRuby = new WUInityColor(132, 63, 91, 255);
        private static readonly WUInityColor DeepSaffron = new WUInityColor(255, 153, 51, 255);
        private static readonly WUInityColor DeepSpaceSparkle = new WUInityColor(74, 100, 108, 255);
        private static readonly WUInityColor DeepTaupe = new WUInityColor(126, 94, 96, 255);
        private static readonly WUInityColor DeepTuscanRed = new WUInityColor(102, 66, 77, 255);
        private static readonly WUInityColor DeepViolet = new WUInityColor(51, 0, 102, 255);
        private static readonly WUInityColor Deer = new WUInityColor(186, 135, 89, 255);
        private static readonly WUInityColor Denim = new WUInityColor(21, 96, 189, 255);
        private static readonly WUInityColor DenimBlue = new WUInityColor(34, 67, 182, 255);
        private static readonly WUInityColor DesaturatedCyan = new WUInityColor(102, 153, 153, 255);
        private static readonly WUInityColor DesertSand = new WUInityColor(237, 201, 175, 255);
        private static readonly WUInityColor Desire = new WUInityColor(234, 60, 83, 255);
        private static readonly WUInityColor Diamond = new WUInityColor(185, 242, 255, 255);
        private static readonly WUInityColor DimGray = new WUInityColor(105, 105, 105, 255);
        private static readonly WUInityColor DingyDungeon = new WUInityColor(197, 49, 81, 255);
        private static readonly WUInityColor Dirt = new WUInityColor(155, 118, 83, 255);
        private static readonly WUInityColor DirtyBrown = new WUInityColor(181, 101, 30, 255);
        private static readonly WUInityColor DirtyWhite = new WUInityColor(232, 228, 201, 255);
        private static readonly WUInityColor DodgerBlue = new WUInityColor(30, 144, 255, 255);
        private static readonly WUInityColor DodieYellow = new WUInityColor(254, 246, 91, 255);
        private static readonly WUInityColor DogwoodRose = new WUInityColor(215, 24, 104, 255);
        private static readonly WUInityColor DollarBill = new WUInityColor(133, 187, 101, 255);
        private static readonly WUInityColor DolphinGray = new WUInityColor(130, 142, 132, 255);
        private static readonly WUInityColor DonkeyBrown = new WUInityColor(102, 76, 40, 255);
        private static readonly WUInityColor DukeBlue = new WUInityColor(0, 0, 156, 255);
        private static readonly WUInityColor DustStorm = new WUInityColor(229, 204, 201, 255);
        private static readonly WUInityColor DutchWhite = new WUInityColor(239, 223, 187, 255);
        private static readonly WUInityColor EarthYellow = new WUInityColor(225, 169, 95, 255);
        private static readonly WUInityColor Ebony = new WUInityColor(85, 93, 80, 255);
        private static readonly WUInityColor Ecru = new WUInityColor(194, 178, 128, 255);
        private static readonly WUInityColor EerieBlack = new WUInityColor(27, 27, 27, 255);
        private static readonly WUInityColor Eggplant = new WUInityColor(97, 64, 81, 255);
        private static readonly WUInityColor Eggshell = new WUInityColor(240, 234, 214, 255);
        private static readonly WUInityColor EgyptianBlue = new WUInityColor(16, 52, 166, 255);
        private static readonly WUInityColor ElectricBlue = new WUInityColor(125, 249, 255, 255);
        private static readonly WUInityColor ElectricCrimson = new WUInityColor(255, 0, 63, 255);
        private static readonly WUInityColor ElectricGreen = new WUInityColor(0, 255, 0, 255);
        private static readonly WUInityColor ElectricIndigo = new WUInityColor(111, 0, 255, 255);
        private static readonly WUInityColor ElectricLime = new WUInityColor(204, 255, 0, 255);
        private static readonly WUInityColor ElectricPurple = new WUInityColor(191, 0, 255, 255);
        private static readonly WUInityColor ElectricUltramarine = new WUInityColor(63, 0, 255, 255);
        private static readonly WUInityColor ElectricViolet = new WUInityColor(143, 0, 255, 255);
        private static readonly WUInityColor ElectricYellow = new WUInityColor(255, 255, 51, 255);
        private static readonly WUInityColor Emerald = new WUInityColor(80, 200, 120, 255);
        private static readonly WUInityColor EmeraldGreen = new WUInityColor(4, 99, 7, 255);
        private static readonly WUInityColor Eminence = new WUInityColor(108, 48, 130, 255);
        private static readonly WUInityColor EnglishLavender = new WUInityColor(180, 131, 149, 255);
        private static readonly WUInityColor EnglishRed = new WUInityColor(171, 75, 82, 255);
        private static readonly WUInityColor EnglishVermillion = new WUInityColor(204, 71, 75, 255);
        private static readonly WUInityColor EnglishViolet = new WUInityColor(86, 60, 92, 255);
        private static readonly WUInityColor EtonBlue = new WUInityColor(150, 200, 162, 255);
        private static readonly WUInityColor Eucalyptus = new WUInityColor(68, 215, 168, 255);
        private static readonly WUInityColor FaluRed = new WUInityColor(128, 24, 24, 255);
        private static readonly WUInityColor Fandango = new WUInityColor(181, 51, 137, 255);
        private static readonly WUInityColor FandangoPink = new WUInityColor(222, 82, 133, 255);
        private static readonly WUInityColor FashionFuchsia = new WUInityColor(244, 0, 161, 255);
        private static readonly WUInityColor Fawn = new WUInityColor(229, 170, 112, 255);
        private static readonly WUInityColor Feldgrau = new WUInityColor(77, 93, 83, 255);
        private static readonly WUInityColor Feldspar = new WUInityColor(253, 213, 177, 255);
        private static readonly WUInityColor FernGreen = new WUInityColor(79, 121, 66, 255);
        private static readonly WUInityColor FerrariRed = new WUInityColor(255, 40, 0, 255);
        private static readonly WUInityColor FieldDrab = new WUInityColor(108, 84, 30, 255);
        private static readonly WUInityColor FieryRose = new WUInityColor(255, 84, 112, 255);
        private static readonly WUInityColor Firebrick = new WUInityColor(178, 34, 34, 255);
        private static readonly WUInityColor FireEngineRed = new WUInityColor(206, 32, 41, 255);
        private static readonly WUInityColor FireOpal = new WUInityColor(233, 92, 75, 255);
        private static readonly WUInityColor Flame = new WUInityColor(226, 88, 34, 255);
        private static readonly WUInityColor FlamingoPink = new WUInityColor(252, 142, 172, 255);
        private static readonly WUInityColor Flavescent = new WUInityColor(247, 233, 142, 255);
        private static readonly WUInityColor Flax = new WUInityColor(238, 220, 130, 255);
        private static readonly WUInityColor Flesh = new WUInityColor(255, 233, 209, 255);
        private static readonly WUInityColor Flirt = new WUInityColor(162, 0, 109, 255);
        private static readonly WUInityColor FloralWhite = new WUInityColor(255, 250, 240, 255);
        private static readonly WUInityColor Folly = new WUInityColor(255, 0, 79, 255);
        private static readonly WUInityColor ForestGreenTraditional = new WUInityColor(1, 68, 33, 255);
        private static readonly WUInityColor ForestGreenWeb = new WUInityColor(34, 139, 34, 255);
        private static readonly WUInityColor FrenchBistre = new WUInityColor(133, 109, 77, 255);
        private static readonly WUInityColor FrenchBlue = new WUInityColor(0, 114, 187, 255);
        private static readonly WUInityColor FrenchFuchsia = new WUInityColor(253, 63, 146, 255);
        private static readonly WUInityColor FrenchLilac = new WUInityColor(134, 96, 142, 255);
        private static readonly WUInityColor FrenchLime = new WUInityColor(158, 253, 56, 255);
        private static readonly WUInityColor FrenchPink = new WUInityColor(253, 108, 158, 255);
        private static readonly WUInityColor FrenchPlum = new WUInityColor(129, 20, 83, 255);
        private static readonly WUInityColor FrenchPuce = new WUInityColor(78, 22, 9, 255);
        private static readonly WUInityColor FrenchRaspberry = new WUInityColor(199, 44, 72, 255);
        private static readonly WUInityColor FrenchRose = new WUInityColor(246, 74, 138, 255);
        private static readonly WUInityColor FrenchSkyBlue = new WUInityColor(119, 181, 254, 255);
        private static readonly WUInityColor FrenchViolet = new WUInityColor(136, 6, 206, 255);
        private static readonly WUInityColor FrenchWine = new WUInityColor(172, 30, 68, 255);
        private static readonly WUInityColor FreshAir = new WUInityColor(166, 231, 255, 255);
        private static readonly WUInityColor Frostbite = new WUInityColor(233, 54, 167, 255);
        private static readonly WUInityColor Fuchsia = new WUInityColor(255, 0, 255, 255);
        private static readonly WUInityColor FuchsiaPink = new WUInityColor(255, 119, 255, 255);
        private static readonly WUInityColor FuchsiaPurple = new WUInityColor(204, 57, 123, 255);
        private static readonly WUInityColor FuchsiaRose = new WUInityColor(199, 67, 117, 255);
        private static readonly WUInityColor Fulvous = new WUInityColor(228, 132, 0, 255);
        private static readonly WUInityColor FuzzyWuzzy = new WUInityColor(204, 102, 102, 255);
        private static readonly WUInityColor Gainsboro = new WUInityColor(220, 220, 220, 255);
        private static readonly WUInityColor Gamboge = new WUInityColor(228, 155, 15, 255);
        private static readonly WUInityColor GambogeOrangeBrown = new WUInityColor(152, 102, 0, 255);
        private static readonly WUInityColor Garnet = new WUInityColor(115, 54, 53, 255);
        private static readonly WUInityColor GargoyleGas = new WUInityColor(255, 223, 70, 255);
        private static readonly WUInityColor GenericViridian = new WUInityColor(0, 127, 102, 255);
        private static readonly WUInityColor GhostWhite = new WUInityColor(248, 248, 255, 255);
        private static readonly WUInityColor GiantsClub = new WUInityColor(176, 92, 82, 255);
        private static readonly WUInityColor GiantsOrange = new WUInityColor(254, 90, 29, 255);
        private static readonly WUInityColor Glaucous = new WUInityColor(96, 130, 182, 255);
        private static readonly WUInityColor GlossyGrape = new WUInityColor(171, 146, 179, 255);
        private static readonly WUInityColor GOGreen = new WUInityColor(0, 171, 102, 255);
        private static readonly WUInityColor Gold = new WUInityColor(165, 124, 0, 255);
        private static readonly WUInityColor GoldMetallic = new WUInityColor(212, 175, 55, 255);
        private static readonly WUInityColor GoldWebGolden = new WUInityColor(255, 215, 0, 255);
        private static readonly WUInityColor GoldCrayola = new WUInityColor(230, 190, 138, 255);
        private static readonly WUInityColor GoldFusion = new WUInityColor(133, 117, 78, 255);
        private static readonly WUInityColor GoldFoil = new WUInityColor(189, 155, 22, 255);
        private static readonly WUInityColor GoldenBrown = new WUInityColor(153, 101, 21, 255);
        private static readonly WUInityColor GoldenPoppy = new WUInityColor(252, 194, 0, 255);
        private static readonly WUInityColor GoldenYellow = new WUInityColor(255, 223, 0, 255);
        private static readonly WUInityColor Goldenrod = new WUInityColor(218, 165, 32, 255);
        private static readonly WUInityColor GraniteGray = new WUInityColor(103, 103, 103, 255);
        private static readonly WUInityColor GrannySmithApple = new WUInityColor(168, 228, 160, 255);
        private static readonly WUInityColor Grape = new WUInityColor(111, 45, 168, 255);
        private static readonly WUInityColor GrayHTMLCSSGray = new WUInityColor(128, 128, 128, 255);
        private static readonly WUInityColor GrayX11Gray = new WUInityColor(190, 190, 190, 255);
        private static readonly WUInityColor GrayAsparagus = new WUInityColor(70, 89, 69, 255);
        private static readonly WUInityColor Green = new WUInityColor(0, 128, 1, 255);
        private static readonly WUInityColor GreenCrayola = new WUInityColor(28, 172, 120, 255);
        private static readonly WUInityColor GreenMunsell = new WUInityColor(0, 168, 119, 255);
        private static readonly WUInityColor GreenNCS = new WUInityColor(0, 159, 107, 255);
        private static readonly WUInityColor GreenPantone = new WUInityColor(0, 173, 67, 255);
        private static readonly WUInityColor GreenPigment = new WUInityColor(0, 165, 80, 255);
        private static readonly WUInityColor GreenRYB = new WUInityColor(102, 176, 50, 255);
        private static readonly WUInityColor GreenBlue = new WUInityColor(17, 100, 180, 255);
        private static readonly WUInityColor GreenCyan = new WUInityColor(0, 153, 102, 255);
        private static readonly WUInityColor GreenLizard = new WUInityColor(167, 244, 50, 255);
        private static readonly WUInityColor GreenSheen = new WUInityColor(110, 174, 161, 255);
        private static readonly WUInityColor GreenYellow = new WUInityColor(173, 255, 47, 255);
        private static readonly WUInityColor Grullo = new WUInityColor(169, 154, 134, 255);
        private static readonly WUInityColor GuppieGreen = new WUInityColor(0, 255, 127, 255);
        private static readonly WUInityColor Gunmetal = new WUInityColor(42, 52, 57, 255);
        private static readonly WUInityColor HalayàÚbe = new WUInityColor(102, 55, 84, 255);
        private static readonly WUInityColor HalloweenOrange = new WUInityColor(235, 97, 35, 255);
        private static readonly WUInityColor HanBlue = new WUInityColor(68, 108, 207, 255);
        private static readonly WUInityColor HanPurple = new WUInityColor(82, 24, 250, 255);
        private static readonly WUInityColor Harlequin = new WUInityColor(63, 255, 0, 255);
        private static readonly WUInityColor HarlequinGreen = new WUInityColor(70, 203, 24, 255);
        private static readonly WUInityColor HarvardCrimson = new WUInityColor(201, 0, 22, 255);
        private static readonly WUInityColor HarvestGold = new WUInityColor(218, 145, 0, 255);
        private static readonly WUInityColor HeartGold = new WUInityColor(128, 128, 0, 255);
        private static readonly WUInityColor HeatWave = new WUInityColor(255, 122, 0, 255);
        private static readonly WUInityColor Heliotrope = new WUInityColor(223, 115, 255, 255);
        private static readonly WUInityColor HeliotropeGray = new WUInityColor(170, 152, 168, 255);
        private static readonly WUInityColor HeliotropeMagenta = new WUInityColor(170, 0, 187, 255);
        private static readonly WUInityColor Honeydew = new WUInityColor(240, 255, 240, 255);
        private static readonly WUInityColor HonoluluBlue = new WUInityColor(0, 109, 176, 255);
        private static readonly WUInityColor HookersGreen = new WUInityColor(73, 121, 107, 255);
        private static readonly WUInityColor HotMagenta = new WUInityColor(255, 29, 206, 255);
        private static readonly WUInityColor HotPink = new WUInityColor(255, 105, 180, 255);
        private static readonly WUInityColor Iceberg = new WUInityColor(113, 166, 210, 255);
        private static readonly WUInityColor Icterine = new WUInityColor(252, 247, 94, 255);
        private static readonly WUInityColor IguanaGreen = new WUInityColor(113, 188, 120, 255);
        private static readonly WUInityColor IlluminatingEmerald = new WUInityColor(49, 145, 119, 255);
        private static readonly WUInityColor Imperial = new WUInityColor(96, 47, 107, 255);
        private static readonly WUInityColor ImperialBlue = new WUInityColor(0, 35, 149, 255);
        private static readonly WUInityColor ImperialPurple = new WUInityColor(102, 2, 60, 255);
        private static readonly WUInityColor ImperialRed = new WUInityColor(237, 41, 57, 255);
        private static readonly WUInityColor Inchworm = new WUInityColor(178, 236, 93, 255);
        private static readonly WUInityColor Independence = new WUInityColor(76, 81, 109, 255);
        private static readonly WUInityColor IndiaGreen = new WUInityColor(19, 136, 8, 255);
        private static readonly WUInityColor IndianRed = new WUInityColor(205, 92, 92, 255);
        private static readonly WUInityColor IndianYellow = new WUInityColor(227, 168, 87, 255);
        private static readonly WUInityColor Indigo = new WUInityColor(75, 0, 130, 255);
        private static readonly WUInityColor IndigoDye = new WUInityColor(9, 31, 146, 255);
        private static readonly WUInityColor IndigoRainbow = new WUInityColor(35, 48, 103, 255);
        private static readonly WUInityColor InfraRed = new WUInityColor(255, 73, 108, 255);
        private static readonly WUInityColor InterdimensionalBlue = new WUInityColor(54, 12, 204, 255);
        private static readonly WUInityColor InternationalKleinBlue = new WUInityColor(0, 47, 167, 255);
        private static readonly WUInityColor InternationalOrangeAerospace = new WUInityColor(255, 79, 0, 255);
        private static readonly WUInityColor InternationalOrangeEngineering = new WUInityColor(186, 22, 12, 255);
        private static readonly WUInityColor InternationalOrangeGoldenGateBridge = new WUInityColor(192, 54, 44, 255);
        private static readonly WUInityColor Iris = new WUInityColor(90, 79, 207, 255);
        private static readonly WUInityColor Irresistible = new WUInityColor(179, 68, 108, 255);
        private static readonly WUInityColor Isabelline = new WUInityColor(244, 240, 236, 255);
        private static readonly WUInityColor IslamicGreen = new WUInityColor(0, 144, 0, 255);
        private static readonly WUInityColor Ivory = new WUInityColor(255, 255, 240, 255);
        private static readonly WUInityColor Jacarta = new WUInityColor(61, 50, 93, 255);
        private static readonly WUInityColor JackoBean = new WUInityColor(65, 54, 40, 255);
        private static readonly WUInityColor Jade = new WUInityColor(0, 168, 107, 255);
        private static readonly WUInityColor JapaneseCarmine = new WUInityColor(157, 41, 51, 255);
        private static readonly WUInityColor JapaneseIndigo = new WUInityColor(38, 67, 72, 255);
        private static readonly WUInityColor JapaneseLaurel = new WUInityColor(47, 117, 50, 255);
        private static readonly WUInityColor JapaneseViolet = new WUInityColor(91, 50, 86, 255);
        private static readonly WUInityColor Jasmine = new WUInityColor(248, 222, 126, 255);
        private static readonly WUInityColor Jasper = new WUInityColor(215, 59, 62, 255);
        private static readonly WUInityColor JasperOrange = new WUInityColor(223, 145, 79, 255);
        private static readonly WUInityColor JazzberryJam = new WUInityColor(165, 11, 94, 255);
        private static readonly WUInityColor JellyBean = new WUInityColor(218, 97, 78, 255);
        private static readonly WUInityColor JellyBeanBlue = new WUInityColor(68, 121, 142, 255);
        private static readonly WUInityColor Jet = new WUInityColor(52, 52, 52, 255);
        private static readonly WUInityColor JetStream = new WUInityColor(187, 208, 201, 255);
        private static readonly WUInityColor Jonquil = new WUInityColor(244, 202, 22, 255);
        private static readonly WUInityColor JordyBlue = new WUInityColor(138, 185, 241, 255);
        private static readonly WUInityColor JuneBud = new WUInityColor(189, 218, 87, 255);
        private static readonly WUInityColor JungleGreen = new WUInityColor(41, 171, 135, 255);
        private static readonly WUInityColor KellyGreen = new WUInityColor(76, 187, 23, 255);
        private static readonly WUInityColor KenyanCopper = new WUInityColor(124, 28, 5, 255);
        private static readonly WUInityColor Keppel = new WUInityColor(58, 176, 158, 255);
        private static readonly WUInityColor KeyLime = new WUInityColor(232, 244, 140, 255);
        private static readonly WUInityColor KhakiHTMLCSSKhaki = new WUInityColor(195, 176, 145, 255);
        private static readonly WUInityColor KhakiX11LightKhaki = new WUInityColor(240, 230, 140, 255);
        private static readonly WUInityColor Kiwi = new WUInityColor(142, 229, 63, 255);
        private static readonly WUInityColor Kobe = new WUInityColor(136, 45, 23, 255);
        private static readonly WUInityColor Kobi = new WUInityColor(231, 159, 196, 255);
        private static readonly WUInityColor KombuGreen = new WUInityColor(53, 66, 48, 255);
        private static readonly WUInityColor KSUPurple = new WUInityColor(79, 38, 131, 255);
        private static readonly WUInityColor KUCrimson = new WUInityColor(232, 0, 13, 255);
        private static readonly WUInityColor LaSalleGreen = new WUInityColor(8, 120, 48, 255);
        private static readonly WUInityColor LanguidLavender = new WUInityColor(214, 202, 221, 255);
        private static readonly WUInityColor LapisLazuli = new WUInityColor(38, 97, 156, 255);
        private static readonly WUInityColor LaserLemon = new WUInityColor(255, 255, 102, 255);
        private static readonly WUInityColor LaurelGreen = new WUInityColor(169, 186, 157, 255);
        private static readonly WUInityColor Lava = new WUInityColor(207, 16, 32, 255);
        private static readonly WUInityColor LavenderFloral = new WUInityColor(181, 126, 220, 255);
        private static readonly WUInityColor LavenderWeb = new WUInityColor(230, 230, 250, 255);
        private static readonly WUInityColor LavenderBlue = new WUInityColor(204, 204, 255, 255);
        private static readonly WUInityColor LavenderBlush = new WUInityColor(255, 240, 245, 255);
        private static readonly WUInityColor LavenderGray = new WUInityColor(196, 195, 208, 255);
        private static readonly WUInityColor LavenderIndigo = new WUInityColor(148, 87, 235, 255);
        private static readonly WUInityColor LavenderMagenta = new WUInityColor(238, 130, 238, 255);
        private static readonly WUInityColor LavenderPink = new WUInityColor(251, 174, 210, 255);
        private static readonly WUInityColor LavenderPurple = new WUInityColor(150, 123, 182, 255);
        private static readonly WUInityColor LavenderRose = new WUInityColor(251, 160, 227, 255);
        private static readonly WUInityColor LawnGreen = new WUInityColor(124, 252, 0, 255);
        private static readonly WUInityColor Lemon = new WUInityColor(255, 247, 0, 255);
        private static readonly WUInityColor LemonChiffon = new WUInityColor(255, 250, 205, 255);
        private static readonly WUInityColor LemonCurry = new WUInityColor(204, 160, 29, 255);
        private static readonly WUInityColor LemonGlacier = new WUInityColor(253, 255, 0, 255);
        private static readonly WUInityColor LemonMeringue = new WUInityColor(246, 234, 190, 255);
        private static readonly WUInityColor LemonYellow = new WUInityColor(255, 244, 79, 255);
        private static readonly WUInityColor LemonYellowCrayola = new WUInityColor(255, 255, 159, 255);
        private static readonly WUInityColor Lenurple = new WUInityColor(186, 147, 216, 255);
        private static readonly WUInityColor Liberty = new WUInityColor(84, 90, 167, 255);
        private static readonly WUInityColor Licorice = new WUInityColor(26, 17, 16, 255);
        private static readonly WUInityColor LightBlue = new WUInityColor(173, 216, 230, 255);
        private static readonly WUInityColor LightBrown = new WUInityColor(181, 101, 29, 255);
        private static readonly WUInityColor LightCarminePink = new WUInityColor(230, 103, 113, 255);
        private static readonly WUInityColor LightCobaltBlue = new WUInityColor(136, 172, 224, 255);
        private static readonly WUInityColor LightCoral = new WUInityColor(240, 128, 128, 255);
        private static readonly WUInityColor LightCornflowerBlue = new WUInityColor(147, 204, 234, 255);
        private static readonly WUInityColor LightCrimson = new WUInityColor(245, 105, 145, 255);
        private static readonly WUInityColor LightCyan = new WUInityColor(224, 255, 255, 255);
        private static readonly WUInityColor LightDeepPink = new WUInityColor(255, 92, 205, 255);
        private static readonly WUInityColor LightFrenchBeige = new WUInityColor(200, 173, 127, 255);
        private static readonly WUInityColor LightFuchsiaPink = new WUInityColor(249, 132, 239, 255);
        private static readonly WUInityColor LightGold = new WUInityColor(178, 151, 0, 255);
        private static readonly WUInityColor LightGoldenrodYellow = new WUInityColor(250, 250, 210, 255);
        private static readonly WUInityColor LightGray = new WUInityColor(211, 211, 211, 255);
        private static readonly WUInityColor LightGrayishMagenta = new WUInityColor(204, 153, 204, 255);
        private static readonly WUInityColor LightGreen = new WUInityColor(144, 238, 144, 255);
        private static readonly WUInityColor LightHotPink = new WUInityColor(255, 179, 222, 255);
        private static readonly WUInityColor LightMediumOrchid = new WUInityColor(211, 155, 203, 255);
        private static readonly WUInityColor LightMossGreen = new WUInityColor(173, 223, 173, 255);
        private static readonly WUInityColor LightOrange = new WUInityColor(254, 216, 177, 255);
        private static readonly WUInityColor LightOrchid = new WUInityColor(230, 168, 215, 255);
        private static readonly WUInityColor LightPastelPurple = new WUInityColor(177, 156, 217, 255);
        private static readonly WUInityColor LightPeriwinkle = new WUInityColor(197, 203, 225, 255);
        private static readonly WUInityColor LightPink = new WUInityColor(255, 182, 193, 255);
        private static readonly WUInityColor LightSalmon = new WUInityColor(255, 160, 122, 255);
        private static readonly WUInityColor LightSalmonPink = new WUInityColor(255, 153, 153, 255);
        private static readonly WUInityColor LightSeaGreen = new WUInityColor(32, 178, 170, 255);
        private static readonly WUInityColor LightSilver = new WUInityColor(216, 216, 216, 255);
        private static readonly WUInityColor LightSkyBlue = new WUInityColor(135, 206, 250, 255);
        private static readonly WUInityColor LightSlateGray = new WUInityColor(119, 136, 153, 255);
        private static readonly WUInityColor LightSteelBlue = new WUInityColor(176, 196, 222, 255);
        private static readonly WUInityColor LightTaupe = new WUInityColor(179, 139, 109, 255);
        private static readonly WUInityColor LightYellow = new WUInityColor(255, 255, 224, 255);
        private static readonly WUInityColor Lilac = new WUInityColor(200, 162, 200, 255);
        private static readonly WUInityColor LilacLuster = new WUInityColor(174, 152, 170, 255);
        private static readonly WUInityColor LimeGreen = new WUInityColor(50, 205, 50, 255);
        private static readonly WUInityColor Limerick = new WUInityColor(157, 194, 9, 255);
        private static readonly WUInityColor LincolnGreen = new WUInityColor(25, 89, 5, 255);
        private static readonly WUInityColor Linen = new WUInityColor(250, 240, 230, 255);
        private static readonly WUInityColor LittleBoyBlue = new WUInityColor(108, 160, 220, 255);
        private static readonly WUInityColor LittleGirlPink = new WUInityColor(248, 185, 212, 255);
        private static readonly WUInityColor Liver = new WUInityColor(103, 76, 71, 255);
        private static readonly WUInityColor LiverDogs = new WUInityColor(184, 109, 41, 255);
        private static readonly WUInityColor LiverOrgan = new WUInityColor(108, 46, 31, 255);
        private static readonly WUInityColor LiverChestnut = new WUInityColor(152, 116, 86, 255);
        private static readonly WUInityColor Lotion = new WUInityColor(255, 254, 250, 255);
        private static readonly WUInityColor Lumber = new WUInityColor(255, 228, 205, 255);
        private static readonly WUInityColor Lust = new WUInityColor(230, 32, 32, 255);
        private static readonly WUInityColor MaastrichtBlue = new WUInityColor(0, 28, 61, 255);
        private static readonly WUInityColor MacaroniAndCheese = new WUInityColor(255, 189, 136, 255);
        private static readonly WUInityColor MadderLake = new WUInityColor(204, 51, 54, 255);
        private static readonly WUInityColor MagentaDye = new WUInityColor(202, 31, 123, 255);
        private static readonly WUInityColor MagentaPantone = new WUInityColor(208, 65, 126, 255);
        private static readonly WUInityColor MagentaProcess = new WUInityColor(255, 0, 144, 255);
        private static readonly WUInityColor MagentaHaze = new WUInityColor(159, 69, 118, 255);
        private static readonly WUInityColor MagentaPink = new WUInityColor(204, 51, 139, 255);
        private static readonly WUInityColor MagicMint = new WUInityColor(170, 240, 209, 255);
        private static readonly WUInityColor MagicPotion = new WUInityColor(255, 68, 102, 255);
        private static readonly WUInityColor Magnolia = new WUInityColor(248, 244, 255, 255);
        private static readonly WUInityColor Mahogany = new WUInityColor(192, 64, 0, 255);
        private static readonly WUInityColor MaizeCrayola = new WUInityColor(242, 198, 73, 255);
        private static readonly WUInityColor MajorelleBlue = new WUInityColor(96, 80, 220, 255);
        private static readonly WUInityColor Malachite = new WUInityColor(11, 218, 81, 255);
        private static readonly WUInityColor Manatee = new WUInityColor(151, 154, 170, 255);
        private static readonly WUInityColor Mandarin = new WUInityColor(243, 122, 72, 255);
        private static readonly WUInityColor MangoGreen = new WUInityColor(150, 255, 0, 255);
        private static readonly WUInityColor MangoTango = new WUInityColor(255, 130, 67, 255);
        private static readonly WUInityColor Mantis = new WUInityColor(116, 195, 101, 255);
        private static readonly WUInityColor MardiGras = new WUInityColor(136, 0, 133, 255);
        private static readonly WUInityColor Marigold = new WUInityColor(234, 162, 33, 255);
        private static readonly WUInityColor MaroonHTMLCSS = new WUInityColor(128, 0, 0, 255);
        private static readonly WUInityColor MaroonX11 = new WUInityColor(176, 48, 96, 255);
        private static readonly WUInityColor Mauve = new WUInityColor(224, 176, 255, 255);
        private static readonly WUInityColor MauveTaupe = new WUInityColor(145, 95, 109, 255);
        private static readonly WUInityColor Mauvelous = new WUInityColor(239, 152, 170, 255);
        private static readonly WUInityColor MaximumBlue = new WUInityColor(71, 171, 204, 255);
        private static readonly WUInityColor MaximumBlueGreen = new WUInityColor(48, 191, 191, 255);
        private static readonly WUInityColor MaximumBluePurple = new WUInityColor(172, 172, 230, 255);
        private static readonly WUInityColor MaximumGreen = new WUInityColor(94, 140, 49, 255);
        private static readonly WUInityColor MaximumGreenYellow = new WUInityColor(217, 230, 80, 255);
        private static readonly WUInityColor MaximumPurple = new WUInityColor(115, 51, 128, 255);
        private static readonly WUInityColor MaximumRed = new WUInityColor(217, 33, 33, 255);
        private static readonly WUInityColor MaximumRedPurple = new WUInityColor(166, 58, 121, 255);
        private static readonly WUInityColor MaximumYellow = new WUInityColor(250, 250, 55, 255);
        private static readonly WUInityColor MaximumYellowRed = new WUInityColor(242, 186, 73, 255);
        private static readonly WUInityColor MayGreen = new WUInityColor(76, 145, 65, 255);
        private static readonly WUInityColor MayaBlue = new WUInityColor(115, 194, 251, 255);
        private static readonly WUInityColor MeatBrown = new WUInityColor(229, 183, 59, 255);
        private static readonly WUInityColor MediumAquamarine = new WUInityColor(102, 221, 170, 255);
        private static readonly WUInityColor MediumBlue = new WUInityColor(0, 0, 205, 255);
        private static readonly WUInityColor MediumCandyAppleRed = new WUInityColor(226, 6, 44, 255);
        private static readonly WUInityColor MediumCarmine = new WUInityColor(175, 64, 53, 255);
        private static readonly WUInityColor MediumChampagne = new WUInityColor(243, 229, 171, 255);
        private static readonly WUInityColor MediumElectricBlue = new WUInityColor(3, 80, 150, 255);
        private static readonly WUInityColor MediumJungleGreen = new WUInityColor(28, 53, 45, 255);
        private static readonly WUInityColor MediumLavenderMagenta = new WUInityColor(221, 160, 221, 255);
        private static readonly WUInityColor MediumOrchid = new WUInityColor(186, 85, 211, 255);
        private static readonly WUInityColor MediumPersianBlue = new WUInityColor(0, 103, 165, 255);
        private static readonly WUInityColor MediumPurple = new WUInityColor(147, 112, 219, 255);
        private static readonly WUInityColor MediumRedViolet = new WUInityColor(187, 51, 133, 255);
        private static readonly WUInityColor MediumRuby = new WUInityColor(170, 64, 105, 255);
        private static readonly WUInityColor MediumSeaGreen = new WUInityColor(60, 179, 113, 255);
        private static readonly WUInityColor MediumSkyBlue = new WUInityColor(128, 218, 235, 255);
        private static readonly WUInityColor MediumSlateBlue = new WUInityColor(123, 104, 238, 255);
        private static readonly WUInityColor MediumSpringBud = new WUInityColor(201, 220, 135, 255);
        private static readonly WUInityColor MediumSpringGreen = new WUInityColor(0, 250, 154, 255);
        private static readonly WUInityColor MediumTurquoise = new WUInityColor(72, 209, 204, 255);
        private static readonly WUInityColor MediumVermilion = new WUInityColor(217, 96, 59, 255);
        private static readonly WUInityColor MediumVioletRed = new WUInityColor(199, 21, 133, 255);
        private static readonly WUInityColor MellowApricot = new WUInityColor(248, 184, 120, 255);
        private static readonly WUInityColor Melon = new WUInityColor(253, 188, 180, 255);
        private static readonly WUInityColor Menthol = new WUInityColor(193, 249, 162, 255);
        private static readonly WUInityColor MetallicBlue = new WUInityColor(50, 82, 123, 255);
        private static readonly WUInityColor MetallicBronze = new WUInityColor(169, 113, 66, 255);
        private static readonly WUInityColor MetallicBrown = new WUInityColor(172, 67, 19, 255);
        private static readonly WUInityColor MetallicGreen = new WUInityColor(41, 110, 1, 255);
        private static readonly WUInityColor MetallicOrange = new WUInityColor(218, 104, 15, 255);
        private static readonly WUInityColor MetallicPink = new WUInityColor(237, 166, 196, 255);
        private static readonly WUInityColor MetallicRed = new WUInityColor(166, 44, 43, 255);
        private static readonly WUInityColor MetallicSeaweed = new WUInityColor(10, 126, 140, 255);
        private static readonly WUInityColor MetallicSilver = new WUInityColor(168, 169, 173, 255);
        private static readonly WUInityColor MetallicSunburst = new WUInityColor(156, 124, 56, 255);
        private static readonly WUInityColor MetallicViolet = new WUInityColor(90, 10, 145, 255);
        private static readonly WUInityColor MetallicYellow = new WUInityColor(253, 204, 13, 255);
        private static readonly WUInityColor MexicanPink = new WUInityColor(228, 0, 124, 255);
        private static readonly WUInityColor MiddleBlue = new WUInityColor(126, 212, 230, 255);
        private static readonly WUInityColor MiddleBlueGreen = new WUInityColor(141, 217, 204, 255);
        private static readonly WUInityColor MiddleBluePurple = new WUInityColor(139, 114, 190, 255);
        private static readonly WUInityColor MiddleGrey = new WUInityColor(139, 134, 128, 255);
        private static readonly WUInityColor MiddleGreen = new WUInityColor(77, 140, 87, 255);
        private static readonly WUInityColor MiddleGreenYellow = new WUInityColor(172, 191, 96, 255);
        private static readonly WUInityColor MiddlePurple = new WUInityColor(217, 130, 181, 255);
        private static readonly WUInityColor MiddleRed = new WUInityColor(229, 144, 115, 255);
        private static readonly WUInityColor MiddleRedPurple = new WUInityColor(165, 83, 83, 255);
        private static readonly WUInityColor MiddleYellow = new WUInityColor(255, 235, 0, 255);
        private static readonly WUInityColor MiddleYellowRed = new WUInityColor(236, 177, 118, 255);
        private static readonly WUInityColor Midnight = new WUInityColor(112, 38, 112, 255);
        private static readonly WUInityColor MidnightBlue = new WUInityColor(25, 25, 112, 255);
        private static readonly WUInityColor MidnightBlue2 = new WUInityColor(0, 70, 140, 255);
        private static readonly WUInityColor MidnightGreenEagleGreen = new WUInityColor(0, 73, 83, 255);
        private static readonly WUInityColor MikadoYellow = new WUInityColor(255, 196, 12, 255);
        private static readonly WUInityColor Milk = new WUInityColor(253, 255, 245, 255);
        private static readonly WUInityColor MilkChocolate = new WUInityColor(132, 86, 60, 255);
        private static readonly WUInityColor MimiPink = new WUInityColor(255, 218, 233, 255);
        private static readonly WUInityColor Mindaro = new WUInityColor(227, 249, 136, 255);
        private static readonly WUInityColor Ming = new WUInityColor(54, 116, 125, 255);
        private static readonly WUInityColor MinionYellow = new WUInityColor(245, 220, 80, 255);
        private static readonly WUInityColor Mint = new WUInityColor(62, 180, 137, 255);
        private static readonly WUInityColor MintCream = new WUInityColor(245, 255, 250, 255);
        private static readonly WUInityColor MintGreen = new WUInityColor(152, 255, 152, 255);
        private static readonly WUInityColor MistyMoss = new WUInityColor(187, 180, 119, 255);
        private static readonly WUInityColor MistyRose = new WUInityColor(255, 228, 225, 255);
        private static readonly WUInityColor Moonstone = new WUInityColor(58, 168, 193, 255);
        private static readonly WUInityColor MoonstoneBlue = new WUInityColor(115, 169, 194, 255);
        private static readonly WUInityColor MordantRed19 = new WUInityColor(174, 12, 0, 255);
        private static readonly WUInityColor MorningBlue = new WUInityColor(141, 163, 153, 255);
        private static readonly WUInityColor MossGreen = new WUInityColor(138, 154, 91, 255);
        private static readonly WUInityColor MountainMeadow = new WUInityColor(48, 186, 143, 255);
        private static readonly WUInityColor MountbattenPink = new WUInityColor(153, 122, 141, 255);
        private static readonly WUInityColor MSUGreen = new WUInityColor(24, 69, 59, 255);
        private static readonly WUInityColor Mud = new WUInityColor(111, 83, 61, 255);
        private static readonly WUInityColor MughalGreen = new WUInityColor(48, 96, 48, 255);
        private static readonly WUInityColor Mulberry = new WUInityColor(197, 75, 140, 255);
        private static readonly WUInityColor MulberryCrayola = new WUInityColor(200, 80, 155, 255);
        private static readonly WUInityColor Mustard = new WUInityColor(255, 219, 88, 255);
        private static readonly WUInityColor MustardBrown = new WUInityColor(205, 122, 0, 255);
        private static readonly WUInityColor MustardGreen = new WUInityColor(110, 110, 48, 255);
        private static readonly WUInityColor MustardYellow = new WUInityColor(255, 173, 1, 255);
        private static readonly WUInityColor MyrtleGreen = new WUInityColor(49, 120, 115, 255);
        private static readonly WUInityColor Mystic = new WUInityColor(214, 82, 130, 255);
        private static readonly WUInityColor MysticMaroon = new WUInityColor(173, 67, 121, 255);
        private static readonly WUInityColor MysticRed = new WUInityColor(255, 34, 0, 255);
        private static readonly WUInityColor NadeshikoPink = new WUInityColor(246, 173, 198, 255);
        private static readonly WUInityColor NapierGreen = new WUInityColor(42, 128, 0, 255);
        private static readonly WUInityColor NaplesYellow = new WUInityColor(250, 218, 94, 255);
        private static readonly WUInityColor NavajoWhite = new WUInityColor(255, 222, 173, 255);
        private static readonly WUInityColor Navy = new WUInityColor(0, 0, 128, 255);
        private static readonly WUInityColor NeonBlue = new WUInityColor(27, 3, 163, 255);
        private static readonly WUInityColor NeonBrown = new WUInityColor(195, 115, 42, 255);
        private static readonly WUInityColor NeonCarrot = new WUInityColor(255, 163, 67, 255);
        private static readonly WUInityColor NeonCyan = new WUInityColor(0, 254, 252, 255);
        private static readonly WUInityColor NeonFuchsia = new WUInityColor(254, 65, 100, 255);
        private static readonly WUInityColor NeonGold = new WUInityColor(207, 170, 1, 255);
        private static readonly WUInityColor NeonGreen = new WUInityColor(57, 255, 20, 255);
        private static readonly WUInityColor NeonPink = new WUInityColor(254, 52, 126, 255);
        private static readonly WUInityColor NeonRed = new WUInityColor(255, 24, 24, 255);
        private static readonly WUInityColor NeonScarlet = new WUInityColor(255, 38, 3, 255);
        private static readonly WUInityColor NeonTangerine = new WUInityColor(246, 137, 10, 255);
        private static readonly WUInityColor NewCar = new WUInityColor(33, 79, 198, 255);
        private static readonly WUInityColor NewYorkPink = new WUInityColor(215, 131, 127, 255);
        private static readonly WUInityColor Nickel = new WUInityColor(114, 116, 114, 255);
        private static readonly WUInityColor NonPhotoBlue = new WUInityColor(164, 221, 237, 255);
        private static readonly WUInityColor NorthTexasGreen = new WUInityColor(5, 144, 51, 255);
        private static readonly WUInityColor Nyanza = new WUInityColor(233, 255, 219, 255);
        private static readonly WUInityColor OceanBlue = new WUInityColor(79, 66, 181, 255);
        private static readonly WUInityColor OceanBoatBlue = new WUInityColor(0, 119, 190, 255);
        private static readonly WUInityColor OceanGreen = new WUInityColor(72, 191, 145, 255);
        private static readonly WUInityColor Ochre = new WUInityColor(204, 119, 34, 255);
        private static readonly WUInityColor OgreOdor = new WUInityColor(253, 82, 64, 255);
        private static readonly WUInityColor OldBurgundy = new WUInityColor(67, 48, 46, 255);
        private static readonly WUInityColor OldGold = new WUInityColor(207, 181, 59, 255);
        private static readonly WUInityColor OldLace = new WUInityColor(253, 245, 230, 255);
        private static readonly WUInityColor OldLavender = new WUInityColor(121, 104, 120, 255);
        private static readonly WUInityColor OldMauve = new WUInityColor(103, 49, 71, 255);
        private static readonly WUInityColor OldMossGreen = new WUInityColor(134, 126, 54, 255);
        private static readonly WUInityColor OldRose = new WUInityColor(192, 128, 129, 255);
        private static readonly WUInityColor OliveDrab3 = new WUInityColor(107, 142, 35, 255);
        private static readonly WUInityColor OliveDrab7 = new WUInityColor(60, 52, 31, 255);
        private static readonly WUInityColor Olivine = new WUInityColor(154, 185, 115, 255);
        private static readonly WUInityColor Onyx = new WUInityColor(53, 56, 57, 255);
        private static readonly WUInityColor Opal = new WUInityColor(168, 195, 188, 255);
        private static readonly WUInityColor OperaMauve = new WUInityColor(183, 132, 167, 255);
        private static readonly WUInityColor OrangeColorWheel = new WUInityColor(255, 127, 0, 255);
        private static readonly WUInityColor OrangeCrayola = new WUInityColor(255, 117, 56, 255);
        private static readonly WUInityColor OrangePantone = new WUInityColor(255, 88, 0, 255);
        private static readonly WUInityColor OrangeRYB = new WUInityColor(251, 153, 2, 255);
        private static readonly WUInityColor OrangeWeb = new WUInityColor(255, 165, 0, 255);
        private static readonly WUInityColor OrangePeel = new WUInityColor(255, 159, 0, 255);
        private static readonly WUInityColor OrangeRed = new WUInityColor(255, 69, 0, 255);
        private static readonly WUInityColor OrangeSoda = new WUInityColor(250, 91, 61, 255);
        private static readonly WUInityColor OrangeYellow = new WUInityColor(248, 213, 104, 255);
        private static readonly WUInityColor Orchid = new WUInityColor(218, 112, 214, 255);
        private static readonly WUInityColor OrchidPink = new WUInityColor(242, 189, 205, 255);
        private static readonly WUInityColor OriolesOrange = new WUInityColor(251, 79, 20, 255);
        private static readonly WUInityColor OuterSpace = new WUInityColor(65, 74, 76, 255);
        private static readonly WUInityColor OutrageousOrange = new WUInityColor(255, 110, 74, 255);
        private static readonly WUInityColor OxfordBlue = new WUInityColor(0, 33, 71, 255);
        private static readonly WUInityColor Oxley = new WUInityColor(109, 154, 121, 255);
        private static readonly WUInityColor PacificBlue = new WUInityColor(28, 169, 201, 255);
        private static readonly WUInityColor PakistanGreen = new WUInityColor(0, 102, 0, 255);
        private static readonly WUInityColor PalatinateBlue = new WUInityColor(39, 59, 226, 255);
        private static readonly WUInityColor PalatinatePurple = new WUInityColor(104, 40, 96, 255);
        private static readonly WUInityColor PaleBlue = new WUInityColor(175, 238, 238, 255);
        private static readonly WUInityColor PaleBrown = new WUInityColor(152, 118, 84, 255);
        private static readonly WUInityColor PaleCerulean = new WUInityColor(155, 196, 226, 255);
        private static readonly WUInityColor PaleChestnut = new WUInityColor(221, 173, 175, 255);
        private static readonly WUInityColor PaleCornflowerBlue = new WUInityColor(171, 205, 239, 255);
        private static readonly WUInityColor PaleCyan = new WUInityColor(135, 211, 248, 255);
        private static readonly WUInityColor PaleGoldenrod = new WUInityColor(238, 232, 170, 255);
        private static readonly WUInityColor PaleGreen = new WUInityColor(152, 251, 152, 255);
        private static readonly WUInityColor PaleLavender = new WUInityColor(220, 208, 255, 255);
        private static readonly WUInityColor PaleMagenta = new WUInityColor(249, 132, 229, 255);
        private static readonly WUInityColor PaleMagentaPink = new WUInityColor(255, 153, 204, 255);
        private static readonly WUInityColor PalePink = new WUInityColor(250, 218, 221, 255);
        private static readonly WUInityColor PaleRedViolet = new WUInityColor(219, 112, 147, 255);
        private static readonly WUInityColor PaleRobinEggBlue = new WUInityColor(150, 222, 209, 255);
        private static readonly WUInityColor PaleSilver = new WUInityColor(201, 192, 187, 255);
        private static readonly WUInityColor PaleSpringBud = new WUInityColor(236, 235, 189, 255);
        private static readonly WUInityColor PaleTaupe = new WUInityColor(188, 152, 126, 255);
        private static readonly WUInityColor PaleViolet = new WUInityColor(204, 153, 255, 255);
        private static readonly WUInityColor PalmLeaf = new WUInityColor(111, 153, 64, 255);
        private static readonly WUInityColor PansyPurple = new WUInityColor(120, 24, 74, 255);
        private static readonly WUInityColor PaoloVeroneseGreen = new WUInityColor(0, 155, 125, 255);
        private static readonly WUInityColor PapayaWhip = new WUInityColor(255, 239, 213, 255);
        private static readonly WUInityColor ParadisePink = new WUInityColor(230, 62, 98, 255);
        private static readonly WUInityColor ParrotPink = new WUInityColor(217, 152, 160, 255);
        private static readonly WUInityColor PastelBlue = new WUInityColor(174, 198, 207, 255);
        private static readonly WUInityColor PastelBrown = new WUInityColor(130, 105, 83, 255);
        private static readonly WUInityColor PastelGray = new WUInityColor(207, 207, 196, 255);
        private static readonly WUInityColor PastelGreen = new WUInityColor(119, 221, 119, 255);
        private static readonly WUInityColor PastelMagenta = new WUInityColor(244, 154, 194, 255);
        private static readonly WUInityColor PastelOrange = new WUInityColor(255, 179, 71, 255);
        private static readonly WUInityColor PastelPink = new WUInityColor(222, 165, 164, 255);
        private static readonly WUInityColor PastelPurple = new WUInityColor(179, 158, 181, 255);
        private static readonly WUInityColor PastelRed = new WUInityColor(255, 105, 97, 255);
        private static readonly WUInityColor PastelViolet = new WUInityColor(203, 153, 201, 255);
        private static readonly WUInityColor PastelYellow = new WUInityColor(253, 253, 150, 255);
        private static readonly WUInityColor Patriarch = new WUInityColor(128, 0, 128, 255);
        private static readonly WUInityColor Peach = new WUInityColor(255, 229, 180, 255);
        private static readonly WUInityColor PeachOrange = new WUInityColor(255, 204, 153, 255);
        private static readonly WUInityColor PeachPuff = new WUInityColor(255, 218, 185, 255);
        private static readonly WUInityColor PeachYellow = new WUInityColor(250, 223, 173, 255);
        private static readonly WUInityColor Pear = new WUInityColor(209, 226, 49, 255);
        private static readonly WUInityColor Pearl = new WUInityColor(234, 224, 200, 255);
        private static readonly WUInityColor PearlAqua = new WUInityColor(136, 216, 192, 255);
        private static readonly WUInityColor PearlyPurple = new WUInityColor(183, 104, 162, 255);
        private static readonly WUInityColor Peridot = new WUInityColor(230, 226, 0, 255);
        private static readonly WUInityColor PeriwinkleCrayola = new WUInityColor(195, 205, 230, 255);
        private static readonly WUInityColor PermanentGeraniumLake = new WUInityColor(225, 44, 44, 255);
        private static readonly WUInityColor PersianBlue = new WUInityColor(28, 57, 187, 255);
        private static readonly WUInityColor PersianGreen = new WUInityColor(0, 166, 147, 255);
        private static readonly WUInityColor PersianIndigo = new WUInityColor(50, 18, 122, 255);
        private static readonly WUInityColor PersianOrange = new WUInityColor(217, 144, 88, 255);
        private static readonly WUInityColor PersianPink = new WUInityColor(247, 127, 190, 255);
        private static readonly WUInityColor PersianPlum = new WUInityColor(112, 28, 28, 255);
        private static readonly WUInityColor PersianRed = new WUInityColor(204, 51, 51, 255);
        private static readonly WUInityColor PersianRose = new WUInityColor(254, 40, 162, 255);
        private static readonly WUInityColor Persimmon = new WUInityColor(236, 88, 0, 255);
        private static readonly WUInityColor Peru = new WUInityColor(205, 133, 63, 255);
        private static readonly WUInityColor PewterBlue = new WUInityColor(139, 168, 183, 255);
        private static readonly WUInityColor PhilippineBlue = new WUInityColor(0, 56, 167, 255);
        private static readonly WUInityColor PhilippineBrown = new WUInityColor(93, 25, 22, 255);
        private static readonly WUInityColor PhilippineGold = new WUInityColor(177, 115, 4, 255);
        private static readonly WUInityColor PhilippineGoldenYellow = new WUInityColor(253, 223, 22, 255);
        private static readonly WUInityColor PhilippineGray = new WUInityColor(140, 140, 140, 255);
        private static readonly WUInityColor PhilippineGreen = new WUInityColor(0, 133, 67, 255);
        private static readonly WUInityColor PhilippineOrange = new WUInityColor(255, 115, 0, 255);
        private static readonly WUInityColor PhilippinePink = new WUInityColor(255, 26, 142, 255);
        private static readonly WUInityColor PhilippineRed = new WUInityColor(206, 17, 39, 255);
        private static readonly WUInityColor PhilippineSilver = new WUInityColor(179, 179, 179, 255);
        private static readonly WUInityColor PhilippineViolet = new WUInityColor(129, 0, 127, 255);
        private static readonly WUInityColor PhilippineYellow = new WUInityColor(254, 203, 0, 255);
        private static readonly WUInityColor Phlox = new WUInityColor(223, 0, 255, 255);
        private static readonly WUInityColor PhthaloBlue = new WUInityColor(0, 15, 137, 255);
        private static readonly WUInityColor PhthaloGreen = new WUInityColor(18, 53, 36, 255);
        private static readonly WUInityColor PictonBlue = new WUInityColor(69, 177, 232, 255);
        private static readonly WUInityColor PictorialCarmine = new WUInityColor(195, 11, 78, 255);
        private static readonly WUInityColor PiggyPink = new WUInityColor(253, 221, 230, 255);
        private static readonly WUInityColor PineGreen = new WUInityColor(1, 121, 111, 255);
        private static readonly WUInityColor PineTree = new WUInityColor(42, 47, 35, 255);
        private static readonly WUInityColor Pineapple = new WUInityColor(86, 60, 13, 255);
        private static readonly WUInityColor Pink = new WUInityColor(255, 192, 203, 255);
        private static readonly WUInityColor PinkPantone = new WUInityColor(215, 72, 148, 255);
        private static readonly WUInityColor PinkFlamingo = new WUInityColor(252, 116, 253, 255);
        private static readonly WUInityColor PinkLace = new WUInityColor(255, 221, 244, 255);
        private static readonly WUInityColor PinkLavender = new WUInityColor(216, 178, 209, 255);
        private static readonly WUInityColor PinkPearl = new WUInityColor(231, 172, 207, 255);
        private static readonly WUInityColor PinkRaspberry = new WUInityColor(152, 0, 54, 255);
        private static readonly WUInityColor PinkSherbet = new WUInityColor(247, 143, 167, 255);
        private static readonly WUInityColor Pistachio = new WUInityColor(147, 197, 114, 255);
        private static readonly WUInityColor PixiePowder = new WUInityColor(57, 18, 133, 255);
        private static readonly WUInityColor Platinum = new WUInityColor(229, 228, 226, 255);
        private static readonly WUInityColor Plum = new WUInityColor(142, 69, 133, 255);
        private static readonly WUInityColor PlumpPurple = new WUInityColor(89, 70, 178, 255);
        private static readonly WUInityColor PoliceBlue = new WUInityColor(55, 79, 107, 255);
        private static readonly WUInityColor PolishedPine = new WUInityColor(93, 164, 147, 255);
        private static readonly WUInityColor Popstar = new WUInityColor(190, 79, 98, 255);
        private static readonly WUInityColor PortlandOrange = new WUInityColor(255, 90, 54, 255);
        private static readonly WUInityColor PowderBlue = new WUInityColor(176, 224, 230, 255);
        private static readonly WUInityColor PrincessPerfume = new WUInityColor(255, 133, 207, 255);
        private static readonly WUInityColor PrincetonOrange = new WUInityColor(245, 128, 37, 255);
        private static readonly WUInityColor PrussianBlue = new WUInityColor(0, 49, 83, 255);
        private static readonly WUInityColor Puce = new WUInityColor(204, 136, 153, 255);
        private static readonly WUInityColor PuceRed = new WUInityColor(114, 47, 55, 255);
        private static readonly WUInityColor PullmanBrownUPSBrown = new WUInityColor(100, 65, 23, 255);
        private static readonly WUInityColor PullmanGreen = new WUInityColor(59, 51, 28, 255);
        private static readonly WUInityColor Pumpkin = new WUInityColor(255, 117, 24, 255);
        private static readonly WUInityColor PurpleMunsell = new WUInityColor(159, 0, 197, 255);
        private static readonly WUInityColor PurpleX11 = new WUInityColor(160, 32, 240, 255);
        private static readonly WUInityColor PurpleHeart = new WUInityColor(105, 53, 156, 255);
        private static readonly WUInityColor PurpleMountainMajesty = new WUInityColor(150, 120, 182, 255);
        private static readonly WUInityColor PurpleNavy = new WUInityColor(78, 81, 128, 255);
        private static readonly WUInityColor PurplePizzazz = new WUInityColor(254, 78, 218, 255);
        private static readonly WUInityColor PurplePlum = new WUInityColor(156, 81, 182, 255);
        private static readonly WUInityColor PurpleTaupe = new WUInityColor(80, 64, 77, 255);
        private static readonly WUInityColor Purpureus = new WUInityColor(154, 78, 174, 255);
        private static readonly WUInityColor Quartz = new WUInityColor(81, 72, 79, 255);
        private static readonly WUInityColor QueenBlue = new WUInityColor(67, 107, 149, 255);
        private static readonly WUInityColor QueenPink = new WUInityColor(232, 204, 215, 255);
        private static readonly WUInityColor QuickSilver = new WUInityColor(166, 166, 166, 255);
        private static readonly WUInityColor QuinacridoneMagenta = new WUInityColor(142, 58, 89, 255);
        private static readonly WUInityColor Quincy = new WUInityColor(106, 84, 69, 255);
        private static readonly WUInityColor RadicalRed = new WUInityColor(255, 53, 94, 255);
        private static readonly WUInityColor RaisinBlack = new WUInityColor(36, 33, 36, 255);
        private static readonly WUInityColor Rajah = new WUInityColor(251, 171, 96, 255);
        private static readonly WUInityColor Raspberry = new WUInityColor(227, 11, 92, 255);
        private static readonly WUInityColor RaspberryPink = new WUInityColor(226, 80, 152, 255);
        private static readonly WUInityColor RawSienna = new WUInityColor(214, 138, 89, 255);
        private static readonly WUInityColor RawUmber = new WUInityColor(130, 102, 68, 255);
        private static readonly WUInityColor RazzleDazzleRose = new WUInityColor(255, 51, 204, 255);
        private static readonly WUInityColor Razzmatazz = new WUInityColor(227, 37, 107, 255);
        private static readonly WUInityColor RazzmicBerry = new WUInityColor(141, 78, 133, 255);
        private static readonly WUInityColor RebeccaPurple = new WUInityColor(102, 52, 153, 255);
        private static readonly WUInityColor Red = new WUInityColor(255, 0, 0, 255);
        private static readonly WUInityColor RedCrayola = new WUInityColor(238, 32, 77, 255);
        private static readonly WUInityColor RedMunsell = new WUInityColor(242, 0, 60, 255);
        private static readonly WUInityColor RedNCS = new WUInityColor(196, 2, 51, 255);
        private static readonly WUInityColor RedPigment = new WUInityColor(237, 28, 36, 255);
        private static readonly WUInityColor RedRYB = new WUInityColor(254, 39, 18, 255);
        private static readonly WUInityColor RedDevil = new WUInityColor(134, 1, 17, 255);
        private static readonly WUInityColor RedOrange = new WUInityColor(255, 83, 73, 255);
        private static readonly WUInityColor RedPurple = new WUInityColor(228, 0, 120, 255);
        private static readonly WUInityColor RedSalsa = new WUInityColor(253, 58, 74, 255);
        private static readonly WUInityColor Redwood = new WUInityColor(164, 90, 82, 255);
        private static readonly WUInityColor Regalia = new WUInityColor(82, 45, 128, 255);
        private static readonly WUInityColor ResolutionBlue = new WUInityColor(0, 35, 135, 255);
        private static readonly WUInityColor Rhythm = new WUInityColor(119, 118, 150, 255);
        private static readonly WUInityColor RichBlack = new WUInityColor(0, 64, 64, 255);
        private static readonly WUInityColor RichBlackFOGRA29 = new WUInityColor(1, 11, 19, 255);
        private static readonly WUInityColor RichBlackFOGRA39 = new WUInityColor(1, 2, 3, 255);
        private static readonly WUInityColor RichBrilliantLavender = new WUInityColor(241, 167, 254, 255);
        private static readonly WUInityColor RichElectricBlue = new WUInityColor(8, 146, 208, 255);
        private static readonly WUInityColor RichLavender = new WUInityColor(167, 107, 207, 255);
        private static readonly WUInityColor RichLilac = new WUInityColor(182, 102, 210, 255);
        private static readonly WUInityColor RifleGreen = new WUInityColor(68, 76, 56, 255);
        private static readonly WUInityColor RobinEggBlue = new WUInityColor(0, 204, 204, 255);
        private static readonly WUInityColor RocketMetallic = new WUInityColor(138, 127, 128, 255);
        private static readonly WUInityColor RomanSilver = new WUInityColor(131, 137, 150, 255);
        private static readonly WUInityColor RootBeer = new WUInityColor(41, 14, 5, 255);
        private static readonly WUInityColor RoseBonbon = new WUInityColor(249, 66, 158, 255);
        private static readonly WUInityColor RoseDust = new WUInityColor(158, 94, 111, 255);
        private static readonly WUInityColor RoseEbony = new WUInityColor(103, 72, 70, 255);
        private static readonly WUInityColor RoseGarnet = new WUInityColor(150, 1, 69, 255);
        private static readonly WUInityColor RoseGold = new WUInityColor(183, 110, 121, 255);
        private static readonly WUInityColor RosePink = new WUInityColor(255, 102, 204, 255);
        private static readonly WUInityColor RoseQuartz = new WUInityColor(170, 152, 169, 255);
        private static readonly WUInityColor RoseQuartzPink = new WUInityColor(189, 85, 156, 255);
        private static readonly WUInityColor RoseRed = new WUInityColor(194, 30, 86, 255);
        private static readonly WUInityColor RoseTaupe = new WUInityColor(144, 93, 93, 255);
        private static readonly WUInityColor RoseVale = new WUInityColor(171, 78, 82, 255);
        private static readonly WUInityColor Rosewood = new WUInityColor(101, 0, 11, 255);
        private static readonly WUInityColor RossoCorsa = new WUInityColor(212, 0, 0, 255);
        private static readonly WUInityColor RosyBrown = new WUInityColor(188, 143, 143, 255);
        private static readonly WUInityColor RoyalAzure = new WUInityColor(0, 56, 168, 255);
        private static readonly WUInityColor RoyalBlue = new WUInityColor(0, 35, 102, 255);
        private static readonly WUInityColor RoyalBlue2 = new WUInityColor(65, 105, 225, 255);
        private static readonly WUInityColor RoyalBrown = new WUInityColor(82, 59, 53, 255);
        private static readonly WUInityColor RoyalFuchsia = new WUInityColor(202, 44, 146, 255);
        private static readonly WUInityColor RoyalGreen = new WUInityColor(19, 98, 7, 255);
        private static readonly WUInityColor RoyalOrange = new WUInityColor(249, 146, 69, 255);
        private static readonly WUInityColor RoyalPink = new WUInityColor(231, 56, 149, 255);
        private static readonly WUInityColor RoyalRed = new WUInityColor(155, 28, 49, 255);
        private static readonly WUInityColor RoyalRed2 = new WUInityColor(208, 0, 96, 255);
        private static readonly WUInityColor RoyalPurple = new WUInityColor(120, 81, 169, 255);
        private static readonly WUInityColor Ruber = new WUInityColor(206, 70, 118, 255);
        private static readonly WUInityColor RubineRed = new WUInityColor(209, 0, 86, 255);
        private static readonly WUInityColor Ruby = new WUInityColor(224, 17, 95, 255);
        private static readonly WUInityColor RubyRed = new WUInityColor(155, 17, 30, 255);
        private static readonly WUInityColor Ruddy = new WUInityColor(255, 0, 40, 255);
        private static readonly WUInityColor RuddyBrown = new WUInityColor(187, 101, 40, 255);
        private static readonly WUInityColor RuddyPink = new WUInityColor(225, 142, 150, 255);
        private static readonly WUInityColor Rufous = new WUInityColor(168, 28, 7, 255);
        private static readonly WUInityColor Russet = new WUInityColor(128, 70, 27, 255);
        private static readonly WUInityColor RussianGreen = new WUInityColor(103, 146, 103, 255);
        private static readonly WUInityColor RussianViolet = new WUInityColor(50, 23, 77, 255);
        private static readonly WUInityColor Rust = new WUInityColor(183, 65, 14, 255);
        private static readonly WUInityColor RustyRed = new WUInityColor(218, 44, 67, 255);
        private static readonly WUInityColor SacramentoStateGreen = new WUInityColor(4, 57, 39, 255);
        private static readonly WUInityColor SaddleBrown = new WUInityColor(139, 69, 19, 255);
        private static readonly WUInityColor SafetyOrange = new WUInityColor(255, 120, 0, 255);
        private static readonly WUInityColor SafetyOrangeBlazeOrange = new WUInityColor(255, 103, 0, 255);
        private static readonly WUInityColor SafetyYellow = new WUInityColor(238, 210, 2, 255);
        private static readonly WUInityColor Saffron = new WUInityColor(244, 196, 48, 255);
        private static readonly WUInityColor Sage = new WUInityColor(188, 184, 138, 255);
        private static readonly WUInityColor StPatricksBlue = new WUInityColor(35, 41, 122, 255);
        private static readonly WUInityColor SalemColor = new WUInityColor(23, 123, 77, 255);
        private static readonly WUInityColor Salmon = new WUInityColor(250, 128, 114, 255);
        private static readonly WUInityColor SalmonPink = new WUInityColor(255, 145, 164, 255);
        private static readonly WUInityColor Sandstorm = new WUInityColor(236, 213, 64, 255);
        private static readonly WUInityColor SandyBrown = new WUInityColor(244, 164, 96, 255);
        private static readonly WUInityColor SandyTan = new WUInityColor(253, 217, 181, 255);
        private static readonly WUInityColor Sangria = new WUInityColor(146, 0, 10, 255);
        private static readonly WUInityColor SapGreen = new WUInityColor(80, 125, 42, 255);
        private static readonly WUInityColor Sapphire = new WUInityColor(15, 82, 186, 255);
        private static readonly WUInityColor SasquatchSocks = new WUInityColor(255, 70, 129, 255);
        private static readonly WUInityColor SatinSheenGold = new WUInityColor(203, 161, 53, 255);
        private static readonly WUInityColor Scarlet = new WUInityColor(255, 36, 0, 255);
        private static readonly WUInityColor Scarlet2 = new WUInityColor(253, 14, 53, 255);
        private static readonly WUInityColor SchoolBusYellow = new WUInityColor(255, 216, 0, 255);
        private static readonly WUInityColor ScreaminGreen = new WUInityColor(102, 255, 102, 255);
        private static readonly WUInityColor SeaBlue = new WUInityColor(0, 105, 148, 255);
        private static readonly WUInityColor SeaFoamGreen = new WUInityColor(195, 226, 191, 255);
        private static readonly WUInityColor SeaGreen = new WUInityColor(46, 139, 87, 255);
        private static readonly WUInityColor SeaGreenCrayola = new WUInityColor(1, 255, 205, 255);
        private static readonly WUInityColor SeaSerpent = new WUInityColor(75, 199, 207, 255);
        private static readonly WUInityColor SealBrown = new WUInityColor(50, 20, 20, 255);
        private static readonly WUInityColor Seashell = new WUInityColor(255, 245, 238, 255);
        private static readonly WUInityColor SelectiveYellow = new WUInityColor(255, 186, 0, 255);
        private static readonly WUInityColor Sepia = new WUInityColor(112, 66, 20, 255);
        private static readonly WUInityColor Shadow = new WUInityColor(138, 121, 93, 255);
        private static readonly WUInityColor ShadowBlue = new WUInityColor(119, 139, 165, 255);
        private static readonly WUInityColor Shampoo = new WUInityColor(255, 207, 241, 255);
        private static readonly WUInityColor ShamrockGreen = new WUInityColor(0, 158, 96, 255);
        private static readonly WUInityColor SheenGreen = new WUInityColor(143, 212, 0, 255);
        private static readonly WUInityColor ShimmeringBlush = new WUInityColor(217, 134, 149, 255);
        private static readonly WUInityColor ShinyShamrock = new WUInityColor(95, 167, 120, 255);
        private static readonly WUInityColor ShockingPink = new WUInityColor(252, 15, 192, 255);
        private static readonly WUInityColor ShockingPinkCrayola = new WUInityColor(255, 111, 255, 255);
        private static readonly WUInityColor Silver = new WUInityColor(192, 192, 192, 255);
        private static readonly WUInityColor SilverMetallic = new WUInityColor(170, 169, 173, 255);
        private static readonly WUInityColor SilverChalice = new WUInityColor(172, 172, 172, 255);
        private static readonly WUInityColor SilverFoil = new WUInityColor(175, 177, 174, 255);
        private static readonly WUInityColor SilverLakeBlue = new WUInityColor(93, 137, 186, 255);
        private static readonly WUInityColor SilverPink = new WUInityColor(196, 174, 173, 255);
        private static readonly WUInityColor SilverSand = new WUInityColor(191, 193, 194, 255);
        private static readonly WUInityColor Sinopia = new WUInityColor(203, 65, 11, 255);
        private static readonly WUInityColor SizzlingRed = new WUInityColor(255, 56, 85, 255);
        private static readonly WUInityColor SizzlingSunrise = new WUInityColor(255, 219, 0, 255);
        private static readonly WUInityColor Skobeloff = new WUInityColor(0, 116, 116, 255);
        private static readonly WUInityColor SkyBlue = new WUInityColor(135, 206, 235, 255);
        private static readonly WUInityColor SkyBlueCrayola = new WUInityColor(118, 215, 234, 255);
        private static readonly WUInityColor SkyMagenta = new WUInityColor(207, 113, 175, 255);
        private static readonly WUInityColor SlateBlue = new WUInityColor(106, 90, 205, 255);
        private static readonly WUInityColor SlateGray = new WUInityColor(112, 128, 144, 255);
        private static readonly WUInityColor SlimyGreen = new WUInityColor(41, 150, 23, 255);
        private static readonly WUInityColor SmashedPumpkin = new WUInityColor(255, 109, 58, 255);
        private static readonly WUInityColor Smitten = new WUInityColor(200, 65, 134, 255);
        private static readonly WUInityColor Smoke = new WUInityColor(115, 130, 118, 255);
        private static readonly WUInityColor SmokeyTopaz = new WUInityColor(131, 42, 34, 255);
        private static readonly WUInityColor SmokyBlack = new WUInityColor(16, 12, 8, 255);
        private static readonly WUInityColor SmokyTopaz = new WUInityColor(147, 61, 65, 255);
        private static readonly WUInityColor Snow = new WUInityColor(255, 250, 250, 255);
        private static readonly WUInityColor Soap = new WUInityColor(206, 200, 239, 255);
        private static readonly WUInityColor SoldierGreen = new WUInityColor(84, 90, 44, 255);
        private static readonly WUInityColor SolidPink = new WUInityColor(137, 56, 67, 255);
        private static readonly WUInityColor SonicSilver = new WUInityColor(117, 117, 117, 255);
        private static readonly WUInityColor SpartanCrimson = new WUInityColor(158, 19, 22, 255);
        private static readonly WUInityColor SpaceCadet = new WUInityColor(29, 41, 81, 255);
        private static readonly WUInityColor SpanishBistre = new WUInityColor(128, 117, 50, 255);
        private static readonly WUInityColor SpanishBlue = new WUInityColor(0, 112, 184, 255);
        private static readonly WUInityColor SpanishCarmine = new WUInityColor(209, 0, 71, 255);
        private static readonly WUInityColor SpanishCrimson = new WUInityColor(229, 26, 76, 255);
        private static readonly WUInityColor SpanishGray = new WUInityColor(152, 152, 152, 255);
        private static readonly WUInityColor SpanishGreen = new WUInityColor(0, 145, 80, 255);
        private static readonly WUInityColor SpanishOrange = new WUInityColor(232, 97, 0, 255);
        private static readonly WUInityColor SpanishPink = new WUInityColor(247, 191, 190, 255);
        private static readonly WUInityColor SpanishPurple = new WUInityColor(102, 3, 60, 255);
        private static readonly WUInityColor SpanishRed = new WUInityColor(230, 0, 38, 255);
        private static readonly WUInityColor SpanishViolet = new WUInityColor(76, 40, 130, 255);
        private static readonly WUInityColor SpanishViridian = new WUInityColor(0, 127, 92, 255);
        private static readonly WUInityColor SpanishYellow = new WUInityColor(246, 181, 17, 255);
        private static readonly WUInityColor SpicyMix = new WUInityColor(139, 95, 77, 255);
        private static readonly WUInityColor SpiroDiscoBall = new WUInityColor(15, 192, 252, 255);
        private static readonly WUInityColor SpringBud = new WUInityColor(167, 252, 0, 255);
        private static readonly WUInityColor SpringFrost = new WUInityColor(135, 255, 42, 255);
        private static readonly WUInityColor StarCommandBlue = new WUInityColor(0, 123, 184, 255);
        private static readonly WUInityColor SteelBlue = new WUInityColor(70, 130, 180, 255);
        private static readonly WUInityColor SteelPink = new WUInityColor(204, 51, 204, 255);
        private static readonly WUInityColor SteelTeal = new WUInityColor(95, 138, 139, 255);
        private static readonly WUInityColor Stormcloud = new WUInityColor(79, 102, 106, 255);
        private static readonly WUInityColor Straw = new WUInityColor(228, 217, 111, 255);
        private static readonly WUInityColor Strawberry = new WUInityColor(252, 90, 141, 255);
        private static readonly WUInityColor SugarPlum = new WUInityColor(145, 78, 117, 255);
        private static readonly WUInityColor SunburntCyclops = new WUInityColor(255, 64, 76, 255);
        private static readonly WUInityColor Sunglow = new WUInityColor(255, 204, 51, 255);
        private static readonly WUInityColor Sunny = new WUInityColor(242, 242, 122, 255);
        private static readonly WUInityColor Sunray = new WUInityColor(227, 171, 87, 255);
        private static readonly WUInityColor SunsetOrange = new WUInityColor(253, 94, 83, 255);
        private static readonly WUInityColor SuperPink = new WUInityColor(207, 107, 169, 255);
        private static readonly WUInityColor SweetBrown = new WUInityColor(168, 55, 49, 255);
        private static readonly WUInityColor Tan = new WUInityColor(210, 180, 140, 255);
        private static readonly WUInityColor Tangelo = new WUInityColor(249, 77, 0, 255);
        private static readonly WUInityColor Tangerine = new WUInityColor(242, 133, 0, 255);
        private static readonly WUInityColor TartOrange = new WUInityColor(251, 77, 70, 255);
        private static readonly WUInityColor TaupeGray = new WUInityColor(139, 133, 137, 255);
        private static readonly WUInityColor TeaGreen = new WUInityColor(208, 240, 192, 255);
        private static readonly WUInityColor Teal = new WUInityColor(0, 128, 128, 255);
        private static readonly WUInityColor TealBlue = new WUInityColor(54, 117, 136, 255);
        private static readonly WUInityColor TealDeer = new WUInityColor(153, 230, 179, 255);
        private static readonly WUInityColor TealGreen = new WUInityColor(0, 130, 127, 255);
        private static readonly WUInityColor Telemagenta = new WUInityColor(207, 52, 118, 255);
        private static readonly WUInityColor Temptress = new WUInityColor(60, 33, 38, 255);
        private static readonly WUInityColor TennéTawny = new WUInityColor(205, 87, 0, 255);
        private static readonly WUInityColor TerraCotta = new WUInityColor(226, 114, 91, 255);
        private static readonly WUInityColor Thistle = new WUInityColor(216, 191, 216, 255);
        private static readonly WUInityColor TickleMePink = new WUInityColor(252, 137, 172, 255);
        private static readonly WUInityColor TiffanyBlue = new WUInityColor(10, 186, 181, 255);
        private static readonly WUInityColor TigersEye = new WUInityColor(224, 141, 60, 255);
        private static readonly WUInityColor Timberwolf = new WUInityColor(219, 215, 210, 255);
        private static readonly WUInityColor Titanium = new WUInityColor(135, 134, 129, 255);
        private static readonly WUInityColor TitaniumYellow = new WUInityColor(238, 230, 0, 255);
        private static readonly WUInityColor Tomato = new WUInityColor(255, 99, 71, 255);
        private static readonly WUInityColor Toolbox = new WUInityColor(116, 108, 192, 255);
        private static readonly WUInityColor Topaz = new WUInityColor(255, 200, 124, 255);
        private static readonly WUInityColor TropicalRainForest = new WUInityColor(0, 117, 94, 255);
        private static readonly WUInityColor TropicalViolet = new WUInityColor(205, 164, 222, 255);
        private static readonly WUInityColor TrueBlue = new WUInityColor(0, 115, 207, 255);
        private static readonly WUInityColor TuftsBlue = new WUInityColor(62, 142, 222, 255);
        private static readonly WUInityColor Tulip = new WUInityColor(255, 135, 141, 255);
        private static readonly WUInityColor Tumbleweed = new WUInityColor(222, 170, 136, 255);
        private static readonly WUInityColor TurkishRose = new WUInityColor(181, 114, 129, 255);
        private static readonly WUInityColor Turquoise = new WUInityColor(64, 224, 208, 255);
        private static readonly WUInityColor TurquoiseBlue = new WUInityColor(0, 255, 239, 255);
        private static readonly WUInityColor TurquoiseGreen = new WUInityColor(160, 214, 180, 255);
        private static readonly WUInityColor TurquoiseSurf = new WUInityColor(0, 197, 205, 255);
        private static readonly WUInityColor TuscanRed = new WUInityColor(124, 72, 72, 255);
        private static readonly WUInityColor Tuscany = new WUInityColor(192, 153, 153, 255);
        private static readonly WUInityColor TwilightLavender = new WUInityColor(138, 73, 107, 255);
        private static readonly WUInityColor UABlue = new WUInityColor(0, 51, 170, 255);
        private static readonly WUInityColor UARed = new WUInityColor(217, 0, 76, 255);
        private static readonly WUInityColor Ube = new WUInityColor(136, 120, 195, 255);
        private static readonly WUInityColor UCLABlue = new WUInityColor(83, 104, 149, 255);
        private static readonly WUInityColor UCLAGold = new WUInityColor(255, 179, 0, 255);
        private static readonly WUInityColor UERed = new WUInityColor(186, 0, 1, 255);
        private static readonly WUInityColor UFOGreen = new WUInityColor(60, 208, 112, 255);
        private static readonly WUInityColor Ultramarine = new WUInityColor(18, 10, 143, 255);
        private static readonly WUInityColor UltramarineBlue = new WUInityColor(65, 102, 245, 255);
        private static readonly WUInityColor UltraRed = new WUInityColor(252, 108, 133, 255);
        private static readonly WUInityColor Umber = new WUInityColor(99, 81, 71, 255);
        private static readonly WUInityColor UnbleachedSilk = new WUInityColor(255, 221, 202, 255);
        private static readonly WUInityColor UnitedNationsBlue = new WUInityColor(91, 146, 229, 255);
        private static readonly WUInityColor UniversityOfCaliforniaGold = new WUInityColor(183, 135, 39, 255);
        private static readonly WUInityColor UniversityOfTennesseeOrange = new WUInityColor(247, 127, 0, 255);
        private static readonly WUInityColor UPMaroon = new WUInityColor(123, 17, 19, 255);
        private static readonly WUInityColor UpsdellRed = new WUInityColor(174, 32, 41, 255);
        private static readonly WUInityColor Urobilin = new WUInityColor(225, 173, 33, 255);
        private static readonly WUInityColor USAFABlue = new WUInityColor(0, 79, 152, 255);
        private static readonly WUInityColor UtahCrimson = new WUInityColor(211, 0, 63, 255);
        private static readonly WUInityColor VampireBlack = new WUInityColor(8, 8, 8, 255);
        private static readonly WUInityColor VanDykeBrown = new WUInityColor(102, 66, 40, 255);
        private static readonly WUInityColor VanillaIce = new WUInityColor(243, 143, 169, 255);
        private static readonly WUInityColor VegasGold = new WUInityColor(197, 179, 88, 255);
        private static readonly WUInityColor VenetianRed = new WUInityColor(200, 8, 21, 255);
        private static readonly WUInityColor Verdigris = new WUInityColor(67, 179, 174, 255);
        private static readonly WUInityColor Vermilion = new WUInityColor(217, 56, 30, 255);
        private static readonly WUInityColor VerseGreen = new WUInityColor(24, 136, 13, 255);
        private static readonly WUInityColor VeryLightAzure = new WUInityColor(116, 187, 251, 255);
        private static readonly WUInityColor VeryLightBlue = new WUInityColor(102, 102, 255, 255);
        private static readonly WUInityColor VeryLightMalachiteGreen = new WUInityColor(100, 233, 134, 255);
        private static readonly WUInityColor VeryLightTangelo = new WUInityColor(255, 176, 119, 255);
        private static readonly WUInityColor VeryPaleOrange = new WUInityColor(255, 223, 191, 255);
        private static readonly WUInityColor VeryPaleYellow = new WUInityColor(255, 255, 191, 255);
        private static readonly WUInityColor VioletColorWheel = new WUInityColor(127, 0, 255, 255);
        private static readonly WUInityColor VioletCrayola = new WUInityColor(150, 61, 127, 255);
        private static readonly WUInityColor VioletRYB = new WUInityColor(134, 1, 175, 255);
        private static readonly WUInityColor VioletBlue = new WUInityColor(50, 74, 178, 255);
        private static readonly WUInityColor VioletRed = new WUInityColor(247, 83, 148, 255);
        private static readonly WUInityColor ViolinBrown = new WUInityColor(103, 68, 3, 255);
        private static readonly WUInityColor ViridianGreen = new WUInityColor(0, 150, 152, 255);
        private static readonly WUInityColor VistaBlue = new WUInityColor(124, 158, 217, 255);
        private static readonly WUInityColor VividAuburn = new WUInityColor(146, 39, 36, 255);
        private static readonly WUInityColor VividBurgundy = new WUInityColor(159, 29, 53, 255);
        private static readonly WUInityColor VividCerise = new WUInityColor(218, 29, 129, 255);
        private static readonly WUInityColor VividCerulean = new WUInityColor(0, 170, 238, 255);
        private static readonly WUInityColor VividCrimson = new WUInityColor(204, 0, 51, 255);
        private static readonly WUInityColor VividGamboge = new WUInityColor(255, 153, 0, 255);
        private static readonly WUInityColor VividLimeGreen = new WUInityColor(166, 214, 8, 255);
        private static readonly WUInityColor VividMalachite = new WUInityColor(0, 204, 51, 255);
        private static readonly WUInityColor VividMulberry = new WUInityColor(184, 12, 227, 255);
        private static readonly WUInityColor VividOrange = new WUInityColor(255, 95, 0, 255);
        private static readonly WUInityColor VividOrangePeel = new WUInityColor(255, 160, 0, 255);
        private static readonly WUInityColor VividOrchid = new WUInityColor(204, 0, 255, 255);
        private static readonly WUInityColor VividRaspberry = new WUInityColor(255, 0, 108, 255);
        private static readonly WUInityColor VividRed = new WUInityColor(247, 13, 26, 255);
        private static readonly WUInityColor VividRedTangelo = new WUInityColor(223, 97, 36, 255);
        private static readonly WUInityColor VividSkyBlue = new WUInityColor(0, 204, 255, 255);
        private static readonly WUInityColor VividTangelo = new WUInityColor(240, 116, 39, 255);
        private static readonly WUInityColor VividTangerine = new WUInityColor(255, 160, 137, 255);
        private static readonly WUInityColor VividVermilion = new WUInityColor(229, 96, 36, 255);
        private static readonly WUInityColor VividViolet = new WUInityColor(159, 0, 255, 255);
        private static readonly WUInityColor VividYellow = new WUInityColor(255, 227, 2, 255);
        private static readonly WUInityColor Volt = new WUInityColor(205, 255, 0, 255);
        private static readonly WUInityColor WageningenGreen = new WUInityColor(52, 178, 51, 255);
        private static readonly WUInityColor WarmBlack = new WUInityColor(0, 66, 66, 255);
        private static readonly WUInityColor Watermelon = new WUInityColor(240, 92, 133, 255);
        private static readonly WUInityColor WatermelonRed = new WUInityColor(190, 65, 71, 255);
        private static readonly WUInityColor Waterspout = new WUInityColor(164, 244, 249, 255);
        private static readonly WUInityColor WeldonBlue = new WUInityColor(124, 152, 171, 255);
        private static readonly WUInityColor Wenge = new WUInityColor(100, 84, 82, 255);
        private static readonly WUInityColor Wheat = new WUInityColor(245, 222, 179, 255);
        private static readonly WUInityColor White = new WUInityColor(255, 255, 255, 255);
        private static readonly WUInityColor WhiteChocolate = new WUInityColor(237, 230, 214, 255);
        private static readonly WUInityColor WhiteCoffee = new WUInityColor(230, 224, 212, 255);
        private static readonly WUInityColor WildBlueYonder = new WUInityColor(162, 173, 208, 255);
        private static readonly WUInityColor WildOrchid = new WUInityColor(212, 112, 162, 255);
        private static readonly WUInityColor WildStrawberry = new WUInityColor(255, 67, 164, 255);
        private static readonly WUInityColor WillpowerOrange = new WUInityColor(253, 88, 0, 255);
        private static readonly WUInityColor WindsorTan = new WUInityColor(167, 85, 2, 255);
        private static readonly WUInityColor WineRed = new WUInityColor(177, 18, 38, 255);
        private static readonly WUInityColor WinterSky = new WUInityColor(255, 0, 124, 255);
        private static readonly WUInityColor WinterWizard = new WUInityColor(160, 230, 255, 255);
        private static readonly WUInityColor WintergreenDream = new WUInityColor(86, 136, 125, 255);
        private static readonly WUInityColor Wisteria = new WUInityColor(201, 160, 220, 255);
        private static readonly WUInityColor Xanadu = new WUInityColor(115, 134, 120, 255);
        private static readonly WUInityColor YaleBlue = new WUInityColor(15, 77, 146, 255);
        private static readonly WUInityColor YankeesBlue = new WUInityColor(28, 40, 65, 255);
        private static readonly WUInityColor Yellow = new WUInityColor(255, 255, 0, 255);
        private static readonly WUInityColor YellowCrayola = new WUInityColor(252, 232, 131, 255);
        private static readonly WUInityColor YellowMunsell = new WUInityColor(239, 204, 0, 255);
        private static readonly WUInityColor YellowPantone = new WUInityColor(254, 223, 0, 255);
        private static readonly WUInityColor YellowRYB = new WUInityColor(254, 254, 51, 255);
        private static readonly WUInityColor YellowGreen = new WUInityColor(154, 205, 50, 255);
        private static readonly WUInityColor YellowOrange = new WUInityColor(255, 174, 66, 255);
        private static readonly WUInityColor YellowRose = new WUInityColor(255, 240, 0, 255);
        private static readonly WUInityColor Zaffre = new WUInityColor(0, 20, 168, 255);
        private static readonly WUInityColor ZinnwalditeBrown = new WUInityColor(44, 22, 8, 255);
        private static readonly WUInityColor Zomp = new WUInityColor(57, 167, 142, 255);

		private static readonly WUInityColor[] colors =
		{
			AbsoluteZero,
			Acajou,
			AcidGreen,
			Aero,
			AeroBlue,
			AfricanViolet,
			AirForceBlueRAF,
			AirForceBlueUSAF,
			AirSuperiorityBlue,
			AlabamaCrimson,
			Alabaster,
			AliceBlue,
			AlienArmpit,
			AlizarinCrimson,
			AlloyOrange,
			Almond,
			Amaranth,
			AmaranthDeepPurple,
			AmaranthPink,
			AmaranthPurple,
			AmaranthRed,
			Amazon,
			Amazonite,
			Amber,
			AmberSAEECE,
			AmericanBlue,
			AmericanBrown,
			AmericanGold,
			AmericanGreen,
			AmericanOrange,
			AmericanPink,
			AmericanPurple,
			AmericanRed,
			AmericanRose,
			AmericanSilver,
			AmericanViolet,
			AmericanYellow,
			Amethyst,
			AndroidGreen,
			AntiFlashWhite,
			AntiqueBrass,
			AntiqueBronze,
			AntiqueFuchsia,
			AntiqueRuby,
			AntiqueWhite,
			AoEnglish,
			Apple,
			AppleGreen,
			Apricot,
			Aqua,
			Aquamarine,
			ArcticLime,
			ArmyGreen,
			Arsenic,
			Artichoke,
			ArylideYellow,
			AshGray,
			Asparagus,
			AteneoBlue,
			AtomicTangerine,
			Auburn,
			Aureolin,
			Aurometalsaurus,
			Avocado,
			Awesome,
			Axolotl,
			AztecGold,
			Azure,
			AzureWebColor,
			AzureishWhite,
			BabyBlue,
			BabyBlueEyes,
			BabyPink,
			BabyPowder,
			BakerMillerPink,
			BallBlue,
			BananaMania,
			BananaYellow,
			BangladeshGreen,
			BarbiePink,
			BarnRed,
			BatteryChargedBlue,
			BattleshipGrey,
			Bazaar,
			BeauBlue,
			Beaver,
			Begonia,
			Beige,
			BdazzledBlue,
			BigDipORuby,
			BigFootFeet,
			Bisque,
			Bistre,
			BistreBrown,
			BitterLemon,
			BitterLime,
			Bittersweet,
			BittersweetShimmer,
			Black,
			BlackBean,
			BlackChocolate,
			BlackCoffee,
			BlackCoral,
			BlackLeatherJacket,
			BlackOlive,
			Blackberry,
			BlackShadows,
			BlanchedAlmond,
			BlastOffBronze,
			BleuDeFrance,
			BlizzardBlue,
			Blond,
			BloodOrange,
			BloodRed,
			Blue,
			BlueCrayola,
			BlueMunsell,
			BlueNCS,
			BluePantone,
			BluePigment,
			BlueRYB,
			BlueBell,
			BlueBolt,
			BlueGray,
			BlueGreen,
			BlueJeans,
			BlueMagentaViolet,
			BlueSapphire,
			BlueViolet,
			BlueYonder,
			Blueberry,
			Bluebonnet,
			Blush,
			Bole,
			BondiBlue,
			Bone,
			BoogerBuster,
			BostonUniversityRed,
			Boysenberry,
			BrandeisBlue,
			Brass,
			BrickRed,
			BrightGray,
			BrightGreen,
			BrightLavender,
			BrightLilac,
			BrightMaroon,
			BrightNavyBlue,
			BrightPink,
			BrightTurquoise,
			BrightUbe,
			BrightYellowCrayola,
			BrilliantAzure,
			BrilliantLavender,
			BrilliantRose,
			BrinkPink,
			BritishRacingGreen,
			Bronze,
			Bronze2,
			BronzeMetallic,
			BronzeYellow,
			Brown,
			BrownCrayola,
			BrownTraditional,
			BrownNose,
			BrownSugar,
			BrownChocolate,
			BrownCoffee,
			BrownYellow,
			BrunswickGreen,
			BubbleGum,
			Bubbles,
			BudGreen,
			Buff,
			BulgarianRose,
			Burgundy,
			Burlywood,
			BurnishedBrown,
			BurntOrange,
			BurntSienna,
			BurntUmber,
			ButtonBlue,
			Byzantine,
			Byzantium,
			Cadet,
			CadetBlue,
			CadetGrey,
			CadmiumBlue,
			CadmiumGreen,
			CadmiumOrange,
			CadmiumPurple,
			CadmiumRed,
			CadmiumYellow,
			CadmiumViolet,
			CaféAuLait,
			CaféNoir,
			CalPolyPomonaGreen,
			Calamansi,
			CambridgeBlue,
			Camel,
			CameoPink,
			CamouflageGreen,
			Canary,
			CanaryYellow,
			CandyAppleRed,
			CandyPink,
			Capri,
			CaputMortuum,
			Caramel,
			Cardinal,
			CaribbeanGreen,
			Carmine,
			CarmineMP,
			CarminePink,
			CarmineRed,
			CarnationPink,
			Carnelian,
			CarolinaBlue,
			CarrotOrange,
			CastletonGreen,
			CatalinaBlue,
			Catawba,
			CedarChest,
			Ceil,
			Celadon,
			CeladonBlue,
			CeladonGreen,
			Celeste,
			CelestialBlue,
			Cerise,
			CerisePink,
			CeruleanBlue,
			CeruleanFrost,
			CGBlue,
			CGRed,
			Chamoisee,
			Champagne,
			ChampagnePink,
			Charcoal,
			CharlestonGreen,
			Charm,
			CharmPink,
			ChartreuseTraditional,
			ChartreuseWeb,
			Cheese,
			CherryBlossomPink,
			Chestnut,
			ChinaPink,
			ChinaRose,
			ChineseBlack,
			ChineseBlue,
			ChineseBronze,
			ChineseBrown,
			ChineseGreen,
			ChineseGold,
			ChineseOrange,
			ChinesePink,
			ChinesePurple,
			ChineseRed,
			ChineseSilver,
			ChineseViolet,
			ChineseWhite,
			ChineseYellow,
			ChlorophyllGreen,
			ChocolateKisses,
			ChocolateTraditional,
			ChocolateWeb,
			ChristmasBlue,
			ChristmasBrown,
			ChristmasBrown2,
			ChristmasGreen,
			ChristmasGreen2,
			ChristmasGold,
			ChristmasOrange,
			ChristmasOrange2,
			ChristmasPink,
			ChristmasPink2,
			ChristmasPurple,
			ChristmasPurple2,
			ChristmasRed,
			ChristmasRed2,
			ChristmasSilver,
			ChristmasYellow,
			ChristmasYellow2,
			ChromeYellow,
			Cinereous,
			Cinnabar,
			CinnamonSatin,
			Citrine,
			CitrineBrown,
			Citron,
			Claret,
			ClassicRose,
			CobaltBlue,
			Coconut,
			Coffee,
			Cola,
			ColumbiaBlue,
			Conditioner,
			CongoPink,
			CoolBlack,
			CoolGrey,
			CookiesAndCream,
			Copper,
			CopperCrayola,
			CopperPenny,
			CopperRed,
			CopperRose,
			Coquelicot,
			Coral,
			CoralRed,
			CoralReef,
			Cordovan,
			Corn,
			CornflowerBlue,
			Cornsilk,
			CosmicCobalt,
			CosmicLatte,
			CoyoteBrown,
			CottonCandy,
			Cream,
			Crimson,
			CrimsonGlory,
			CrimsonRed,
			Cultured,
			CyanAzure,
			CyanBlueAzure,
			CyanCobaltBlue,
			CyanCornflowerBlue,
			CyanProcess,
			CyberGrape,
			CyberYellow,
			Cyclamen,
			Daffodil,
			Dandelion,
			DarkBlue,
			DarkBlueGray,
			DarkBronze,
			DarkBrown,
			DarkBrownTangelo,
			DarkByzantium,
			DarkCandyAppleRed,
			DarkCerulean,
			DarkCharcoal,
			DarkChestnut,
			DarkChocolate,
			DarkChocolateHersheys,
			DarkCornflowerBlue,
			DarkCoral,
			DarkCyan,
			DarkElectricBlue,
			DarkGoldenrod,
			DarkGrayX11,
			DarkGreen,
			DarkGreenX11,
			DarkGunmetal,
			DarkImperialBlue,
			DarkImperialBlue2,
			DarkJungleGreen,
			DarkKhaki,
			DarkLava,
			DarkLavender,
			DarkLemonLime,
			DarkLiver,
			DarkLiverHorses,
			DarkMagenta,
			DarkMidnightBlue,
			DarkMossGreen,
			DarkOliveGreen,
			DarkOrange,
			DarkOrchid,
			DarkPastelBlue,
			DarkPastelGreen,
			DarkPastelPurple,
			DarkPastelRed,
			DarkPink,
			DarkPowderBlue,
			DarkPuce,
			DarkPurple,
			DarkRaspberry,
			DarkRed,
			DarkSalmon,
			DarkScarlet,
			DarkSeaGreen,
			DarkSienna,
			DarkSkyBlue,
			DarkSlateBlue,
			DarkSlateGray,
			DarkSpringGreen,
			DarkTan,
			DarkTangerine,
			DarkTerraCotta,
			DarkTurquoise,
			DarkVanilla,
			DarkViolet,
			DarkYellow,
			DartmouthGreen,
			DavysGrey,
			DebianRed,
			DeepAmethyst,
			DeepAquamarine,
			DeepCarmine,
			DeepCarminePink,
			DeepCarrotOrange,
			DeepCerise,
			DeepChampagne,
			DeepChestnut,
			DeepCoffee,
			DeepFuchsia,
			DeepGreen,
			DeepGreenCyanTurquoise,
			DeepJungleGreen,
			DeepKoamaru,
			DeepLemon,
			DeepLilac,
			DeepMagenta,
			DeepMaroon,
			DeepMauve,
			DeepMossGreen,
			DeepPeach,
			DeepPink,
			DeepPuce,
			DeepRed,
			DeepRuby,
			DeepSaffron,
			DeepSpaceSparkle,
			DeepTaupe,
			DeepTuscanRed,
			DeepViolet,
			Deer,
			Denim,
			DenimBlue,
			DesaturatedCyan,
			DesertSand,
			Desire,
			Diamond,
			DimGray,
			DingyDungeon,
			Dirt,
			DirtyBrown,
			DirtyWhite,
			DodgerBlue,
			DodieYellow,
			DogwoodRose,
			DollarBill,
			DolphinGray,
			DonkeyBrown,
			DukeBlue,
			DustStorm,
			DutchWhite,
			EarthYellow,
			Ebony,
			Ecru,
			EerieBlack,
			Eggplant,
			Eggshell,
			EgyptianBlue,
			ElectricBlue,
			ElectricCrimson,
			ElectricGreen,
			ElectricIndigo,
			ElectricLime,
			ElectricPurple,
			ElectricUltramarine,
			ElectricViolet,
			ElectricYellow,
			Emerald,
			EmeraldGreen,
			Eminence,
			EnglishLavender,
			EnglishRed,
			EnglishVermillion,
			EnglishViolet,
			EtonBlue,
			Eucalyptus,
			FaluRed,
			Fandango,
			FandangoPink,
			FashionFuchsia,
			Fawn,
			Feldgrau,
			Feldspar,
			FernGreen,
			FerrariRed,
			FieldDrab,
			FieryRose,
			Firebrick,
			FireEngineRed,
			FireOpal,
			Flame,
			FlamingoPink,
			Flavescent,
			Flax,
			Flesh,
			Flirt,
			FloralWhite,
			Folly,
			ForestGreenTraditional,
			ForestGreenWeb,
			FrenchBistre,
			FrenchBlue,
			FrenchFuchsia,
			FrenchLilac,
			FrenchLime,
			FrenchPink,
			FrenchPlum,
			FrenchPuce,
			FrenchRaspberry,
			FrenchRose,
			FrenchSkyBlue,
			FrenchViolet,
			FrenchWine,
			FreshAir,
			Frostbite,
			Fuchsia,
			FuchsiaPink,
			FuchsiaPurple,
			FuchsiaRose,
			Fulvous,
			FuzzyWuzzy,
			Gainsboro,
			Gamboge,
			GambogeOrangeBrown,
			Garnet,
			GargoyleGas,
			GenericViridian,
			GhostWhite,
			GiantsClub,
			GiantsOrange,
			Glaucous,
			GlossyGrape,
			GOGreen,
			Gold,
			GoldMetallic,
			GoldWebGolden,
			GoldCrayola,
			GoldFusion,
			GoldFoil,
			GoldenBrown,
			GoldenPoppy,
			GoldenYellow,
			Goldenrod,
			GraniteGray,
			GrannySmithApple,
			Grape,
			GrayHTMLCSSGray,
			GrayX11Gray,
			GrayAsparagus,
			Green,
			GreenCrayola,
			GreenMunsell,
			GreenNCS,
			GreenPantone,
			GreenPigment,
			GreenRYB,
			GreenBlue,
			GreenCyan,
			GreenLizard,
			GreenSheen,
			GreenYellow,
			Grullo,
			GuppieGreen,
			Gunmetal,
			HalayàÚbe,
			HalloweenOrange,
			HanBlue,
			HanPurple,
			Harlequin,
			HarlequinGreen,
			HarvardCrimson,
			HarvestGold,
			HeartGold,
			HeatWave,
			Heliotrope,
			HeliotropeGray,
			HeliotropeMagenta,
			Honeydew,
			HonoluluBlue,
			HookersGreen,
			HotMagenta,
			HotPink,
			Iceberg,
			Icterine,
			IguanaGreen,
			IlluminatingEmerald,
			Imperial,
			ImperialBlue,
			ImperialPurple,
			ImperialRed,
			Inchworm,
			Independence,
			IndiaGreen,
			IndianRed,
			IndianYellow,
			Indigo,
			IndigoDye,
			IndigoRainbow,
			InfraRed,
			InterdimensionalBlue,
			InternationalKleinBlue,
			InternationalOrangeAerospace,
			InternationalOrangeEngineering,
			InternationalOrangeGoldenGateBridge,
			Iris,
			Irresistible,
			Isabelline,
			IslamicGreen,
			Ivory,
			Jacarta,
			JackoBean,
			Jade,
			JapaneseCarmine,
			JapaneseIndigo,
			JapaneseLaurel,
			JapaneseViolet,
			Jasmine,
			Jasper,
			JasperOrange,
			JazzberryJam,
			JellyBean,
			JellyBeanBlue,
			Jet,
			JetStream,
			Jonquil,
			JordyBlue,
			JuneBud,
			JungleGreen,
			KellyGreen,
			KenyanCopper,
			Keppel,
			KeyLime,
			KhakiHTMLCSSKhaki,
			KhakiX11LightKhaki,
			Kiwi,
			Kobe,
			Kobi,
			KombuGreen,
			KSUPurple,
			KUCrimson,
			LaSalleGreen,
			LanguidLavender,
			LapisLazuli,
			LaserLemon,
			LaurelGreen,
			Lava,
			LavenderFloral,
			LavenderWeb,
			LavenderBlue,
			LavenderBlush,
			LavenderGray,
			LavenderIndigo,
			LavenderMagenta,
			LavenderPink,
			LavenderPurple,
			LavenderRose,
			LawnGreen,
			Lemon,
			LemonChiffon,
			LemonCurry,
			LemonGlacier,
			LemonMeringue,
			LemonYellow,
			LemonYellowCrayola,
			Lenurple,
			Liberty,
			Licorice,
			LightBlue,
			LightBrown,
			LightCarminePink,
			LightCobaltBlue,
			LightCoral,
			LightCornflowerBlue,
			LightCrimson,
			LightCyan,
			LightDeepPink,
			LightFrenchBeige,
			LightFuchsiaPink,
			LightGold,
			LightGoldenrodYellow,
			LightGray,
			LightGrayishMagenta,
			LightGreen,
			LightHotPink,
			LightMediumOrchid,
			LightMossGreen,
			LightOrange,
			LightOrchid,
			LightPastelPurple,
			LightPeriwinkle,
			LightPink,
			LightSalmon,
			LightSalmonPink,
			LightSeaGreen,
			LightSilver,
			LightSkyBlue,
			LightSlateGray,
			LightSteelBlue,
			LightTaupe,
			LightYellow,
			Lilac,
			LilacLuster,
			LimeGreen,
			Limerick,
			LincolnGreen,
			Linen,
			LittleBoyBlue,
			LittleGirlPink,
			Liver,
			LiverDogs,
			LiverOrgan,
			LiverChestnut,
			Lotion,
			Lumber,
			Lust,
			MaastrichtBlue,
			MacaroniAndCheese,
			MadderLake,
			MagentaDye,
			MagentaPantone,
			MagentaProcess,
			MagentaHaze,
			MagentaPink,
			MagicMint,
			MagicPotion,
			Magnolia,
			Mahogany,
			MaizeCrayola,
			MajorelleBlue,
			Malachite,
			Manatee,
			Mandarin,
			MangoGreen,
			MangoTango,
			Mantis,
			MardiGras,
			Marigold,
			MaroonHTMLCSS,
			MaroonX11,
			Mauve,
			MauveTaupe,
			Mauvelous,
			MaximumBlue,
			MaximumBlueGreen,
			MaximumBluePurple,
			MaximumGreen,
			MaximumGreenYellow,
			MaximumPurple,
			MaximumRed,
			MaximumRedPurple,
			MaximumYellow,
			MaximumYellowRed,
			MayGreen,
			MayaBlue,
			MeatBrown,
			MediumAquamarine,
			MediumBlue,
			MediumCandyAppleRed,
			MediumCarmine,
			MediumChampagne,
			MediumElectricBlue,
			MediumJungleGreen,
			MediumLavenderMagenta,
			MediumOrchid,
			MediumPersianBlue,
			MediumPurple,
			MediumRedViolet,
			MediumRuby,
			MediumSeaGreen,
			MediumSkyBlue,
			MediumSlateBlue,
			MediumSpringBud,
			MediumSpringGreen,
			MediumTurquoise,
			MediumVermilion,
			MediumVioletRed,
			MellowApricot,
			Melon,
			Menthol,
			MetallicBlue,
			MetallicBronze,
			MetallicBrown,
			MetallicGreen,
			MetallicOrange,
			MetallicPink,
			MetallicRed,
			MetallicSeaweed,
			MetallicSilver,
			MetallicSunburst,
			MetallicViolet,
			MetallicYellow,
			MexicanPink,
			MiddleBlue,
			MiddleBlueGreen,
			MiddleBluePurple,
			MiddleGrey,
			MiddleGreen,
			MiddleGreenYellow,
			MiddlePurple,
			MiddleRed,
			MiddleRedPurple,
			MiddleYellow,
			MiddleYellowRed,
			Midnight,
			MidnightBlue,
			MidnightBlue2,
			MidnightGreenEagleGreen,
			MikadoYellow,
			Milk,
			MilkChocolate,
			MimiPink,
			Mindaro,
			Ming,
			MinionYellow,
			Mint,
			MintCream,
			MintGreen,
			MistyMoss,
			MistyRose,
			Moonstone,
			MoonstoneBlue,
			MordantRed19,
			MorningBlue,
			MossGreen,
			MountainMeadow,
			MountbattenPink,
			MSUGreen,
			Mud,
			MughalGreen,
			Mulberry,
			MulberryCrayola,
			Mustard,
			MustardBrown,
			MustardGreen,
			MustardYellow,
			MyrtleGreen,
			Mystic,
			MysticMaroon,
			MysticRed,
			NadeshikoPink,
			NapierGreen,
			NaplesYellow,
			NavajoWhite,
			Navy,
			NeonBlue,
			NeonBrown,
			NeonCarrot,
			NeonCyan,
			NeonFuchsia,
			NeonGold,
			NeonGreen,
			NeonPink,
			NeonRed,
			NeonScarlet,
			NeonTangerine,
			NewCar,
			NewYorkPink,
			Nickel,
			NonPhotoBlue,
			NorthTexasGreen,
			Nyanza,
			OceanBlue,
			OceanBoatBlue,
			OceanGreen,
			Ochre,
			OgreOdor,
			OldBurgundy,
			OldGold,
			OldLace,
			OldLavender,
			OldMauve,
			OldMossGreen,
			OldRose,
			OliveDrab3,
			OliveDrab7,
			Olivine,
			Onyx,
			Opal,
			OperaMauve,
			OrangeColorWheel,
			OrangeCrayola,
			OrangePantone,
			OrangeRYB,
			OrangeWeb,
			OrangePeel,
			OrangeRed,
			OrangeSoda,
			OrangeYellow,
			Orchid,
			OrchidPink,
			OriolesOrange,
			OuterSpace,
			OutrageousOrange,
			OxfordBlue,
			Oxley,
			PacificBlue,
			PakistanGreen,
			PalatinateBlue,
			PalatinatePurple,
			PaleBlue,
			PaleBrown,
			PaleCerulean,
			PaleChestnut,
			PaleCornflowerBlue,
			PaleCyan,
			PaleGoldenrod,
			PaleGreen,
			PaleLavender,
			PaleMagenta,
			PaleMagentaPink,
			PalePink,
			PaleRedViolet,
			PaleRobinEggBlue,
			PaleSilver,
			PaleSpringBud,
			PaleTaupe,
			PaleViolet,
			PalmLeaf,
			PansyPurple,
			PaoloVeroneseGreen,
			PapayaWhip,
			ParadisePink,
			ParrotPink,
			PastelBlue,
			PastelBrown,
			PastelGray,
			PastelGreen,
			PastelMagenta,
			PastelOrange,
			PastelPink,
			PastelPurple,
			PastelRed,
			PastelViolet,
			PastelYellow,
			Patriarch,
			Peach,
			PeachOrange,
			PeachPuff,
			PeachYellow,
			Pear,
			Pearl,
			PearlAqua,
			PearlyPurple,
			Peridot,
			PeriwinkleCrayola,
			PermanentGeraniumLake,
			PersianBlue,
			PersianGreen,
			PersianIndigo,
			PersianOrange,
			PersianPink,
			PersianPlum,
			PersianRed,
			PersianRose,
			Persimmon,
			Peru,
			PewterBlue,
			PhilippineBlue,
			PhilippineBrown,
			PhilippineGold,
			PhilippineGoldenYellow,
			PhilippineGray,
			PhilippineGreen,
			PhilippineOrange,
			PhilippinePink,
			PhilippineRed,
			PhilippineSilver,
			PhilippineViolet,
			PhilippineYellow,
			Phlox,
			PhthaloBlue,
			PhthaloGreen,
			PictonBlue,
			PictorialCarmine,
			PiggyPink,
			PineGreen,
			PineTree,
			Pineapple,
			Pink,
			PinkPantone,
			PinkFlamingo,
			PinkLace,
			PinkLavender,
			PinkPearl,
			PinkRaspberry,
			PinkSherbet,
			Pistachio,
			PixiePowder,
			Platinum,
			Plum,
			PlumpPurple,
			PoliceBlue,
			PolishedPine,
			Popstar,
			PortlandOrange,
			PowderBlue,
			PrincessPerfume,
			PrincetonOrange,
			PrussianBlue,
			Puce,
			PuceRed,
			PullmanBrownUPSBrown,
			PullmanGreen,
			Pumpkin,
			PurpleMunsell,
			PurpleX11,
			PurpleHeart,
			PurpleMountainMajesty,
			PurpleNavy,
			PurplePizzazz,
			PurplePlum,
			PurpleTaupe,
			Purpureus,
			Quartz,
			QueenBlue,
			QueenPink,
			QuickSilver,
			QuinacridoneMagenta,
			Quincy,
			RadicalRed,
			RaisinBlack,
			Rajah,
			Raspberry,
			RaspberryPink,
			RawSienna,
			RawUmber,
			RazzleDazzleRose,
			Razzmatazz,
			RazzmicBerry,
			RebeccaPurple,
			Red,
			RedCrayola,
			RedMunsell,
			RedNCS,
			RedPigment,
			RedRYB,
			RedDevil,
			RedOrange,
			RedPurple,
			RedSalsa,
			Redwood,
			Regalia,
			ResolutionBlue,
			Rhythm,
			RichBlack,
			RichBlackFOGRA29,
			RichBlackFOGRA39,
			RichBrilliantLavender,
			RichElectricBlue,
			RichLavender,
			RichLilac,
			RifleGreen,
			RobinEggBlue,
			RocketMetallic,
			RomanSilver,
			RootBeer,
			RoseBonbon,
			RoseDust,
			RoseEbony,
			RoseGarnet,
			RoseGold,
			RosePink,
			RoseQuartz,
			RoseQuartzPink,
			RoseRed,
			RoseTaupe,
			RoseVale,
			Rosewood,
			RossoCorsa,
			RosyBrown,
			RoyalAzure,
			RoyalBlue,
			RoyalBlue2,
			RoyalBrown,
			RoyalFuchsia,
			RoyalGreen,
			RoyalOrange,
			RoyalPink,
			RoyalRed,
			RoyalRed2,
			RoyalPurple,
			Ruber,
			RubineRed,
			Ruby,
			RubyRed,
			Ruddy,
			RuddyBrown,
			RuddyPink,
			Rufous,
			Russet,
			RussianGreen,
			RussianViolet,
			Rust,
			RustyRed,
			SacramentoStateGreen,
			SaddleBrown,
			SafetyOrange,
			SafetyOrangeBlazeOrange,
			SafetyYellow,
			Saffron,
			Sage,
			StPatricksBlue,
			SalemColor,
			Salmon,
			SalmonPink,
			Sandstorm,
			SandyBrown,
			SandyTan,
			Sangria,
			SapGreen,
			Sapphire,
			SasquatchSocks,
			SatinSheenGold,
			Scarlet,
			Scarlet2,
			SchoolBusYellow,
			ScreaminGreen,
			SeaBlue,
			SeaFoamGreen,
			SeaGreen,
			SeaGreenCrayola,
			SeaSerpent,
			SealBrown,
			Seashell,
			SelectiveYellow,
			Sepia,
			Shadow,
			ShadowBlue,
			Shampoo,
			ShamrockGreen,
			SheenGreen,
			ShimmeringBlush,
			ShinyShamrock,
			ShockingPink,
			ShockingPinkCrayola,
			Silver,
			SilverMetallic,
			SilverChalice,
			SilverFoil,
			SilverLakeBlue,
			SilverPink,
			SilverSand,
			Sinopia,
			SizzlingRed,
			SizzlingSunrise,
			Skobeloff,
			SkyBlue,
			SkyBlueCrayola,
			SkyMagenta,
			SlateBlue,
			SlateGray,
			SlimyGreen,
			SmashedPumpkin,
			Smitten,
			Smoke,
			SmokeyTopaz,
			SmokyBlack,
			SmokyTopaz,
			Snow,
			Soap,
			SoldierGreen,
			SolidPink,
			SonicSilver,
			SpartanCrimson,
			SpaceCadet,
			SpanishBistre,
			SpanishBlue,
			SpanishCarmine,
			SpanishCrimson,
			SpanishGray,
			SpanishGreen,
			SpanishOrange,
			SpanishPink,
			SpanishPurple,
			SpanishRed,
			SpanishViolet,
			SpanishViridian,
			SpanishYellow,
			SpicyMix,
			SpiroDiscoBall,
			SpringBud,
			SpringFrost,
			StarCommandBlue,
			SteelBlue,
			SteelPink,
			SteelTeal,
			Stormcloud,
			Straw,
			Strawberry,
			SugarPlum,
			SunburntCyclops,
			Sunglow,
			Sunny,
			Sunray,
			SunsetOrange,
			SuperPink,
			SweetBrown,
			Tan,
			Tangelo,
			Tangerine,
			TartOrange,
			TaupeGray,
			TeaGreen,
			Teal,
			TealBlue,
			TealDeer,
			TealGreen,
			Telemagenta,
			Temptress,
			TennéTawny,
			TerraCotta,
			Thistle,
			TickleMePink,
			TiffanyBlue,
			TigersEye,
			Timberwolf,
			Titanium,
			TitaniumYellow,
			Tomato,
			Toolbox,
			Topaz,
			TropicalRainForest,
			TropicalViolet,
			TrueBlue,
			TuftsBlue,
			Tulip,
			Tumbleweed,
			TurkishRose,
			Turquoise,
			TurquoiseBlue,
			TurquoiseGreen,
			TurquoiseSurf,
			TuscanRed,
			Tuscany,
			TwilightLavender,
			UABlue,
			UARed,
			Ube,
			UCLABlue,
			UCLAGold,
			UERed,
			UFOGreen,
			Ultramarine,
			UltramarineBlue,
			UltraRed,
			Umber,
			UnbleachedSilk,
			UnitedNationsBlue,
			UniversityOfCaliforniaGold,
			UniversityOfTennesseeOrange,
			UPMaroon,
			UpsdellRed,
			Urobilin,
			USAFABlue,
			UtahCrimson,
			VampireBlack,
			VanDykeBrown,
			VanillaIce,
			VegasGold,
			VenetianRed,
			Verdigris,
			Vermilion,
			VerseGreen,
			VeryLightAzure,
			VeryLightBlue,
			VeryLightMalachiteGreen,
			VeryLightTangelo,
			VeryPaleOrange,
			VeryPaleYellow,
			VioletColorWheel,
			VioletCrayola,
			VioletRYB,
			VioletBlue,
			VioletRed,
			ViolinBrown,
			ViridianGreen,
			VistaBlue,
			VividAuburn,
			VividBurgundy,
			VividCerise,
			VividCerulean,
			VividCrimson,
			VividGamboge,
			VividLimeGreen,
			VividMalachite,
			VividMulberry,
			VividOrange,
			VividOrangePeel,
			VividOrchid,
			VividRaspberry,
			VividRed,
			VividRedTangelo,
			VividSkyBlue,
			VividTangelo,
			VividTangerine,
			VividVermilion,
			VividViolet,
			VividYellow,
			Volt,
			WageningenGreen,
			WarmBlack,
			Watermelon,
			WatermelonRed,
			Waterspout,
			WeldonBlue,
			Wenge,
			Wheat,
			White,
			WhiteChocolate,
			WhiteCoffee,
			WildBlueYonder,
			WildOrchid,
			WildStrawberry,
			WillpowerOrange,
			WindsorTan,
			WineRed,
			WinterSky,
			WinterWizard,
			WintergreenDream,
			Wisteria,
			Xanadu,
			YaleBlue,
			YankeesBlue,
			Yellow,
			YellowCrayola,
			YellowMunsell,
			YellowPantone,
			YellowRYB,
			YellowGreen,
			YellowOrange,
			YellowRose,
			Zaffre,
			ZinnwalditeBrown,
			Zomp
		};
	}
}
