//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace WUIPlatform.Visualization
{
	public static class FuelModelColors
	{
		private static WUIEngineColor ERROR_COLOR = new WUIEngineColor(1.0f, 0, 1.0f);
		public static WUIEngineColor GetFuelColor(int fuelNumber)
		{
			if (fuelNumber > 0 && fuelNumber <= colors.Length)
			{
				WUIEngineColor c = colors[fuelNumber - 1];
				return c;
			}
			return ERROR_COLOR;
		}

        private static readonly WUIEngineColor AbsoluteZero = new WUIEngineColor(0, 72, 186, 255);
        private static readonly WUIEngineColor Acajou = new WUIEngineColor(76, 47, 39, 255);
        private static readonly WUIEngineColor AcidGreen = new WUIEngineColor(176, 191, 26, 255);
        private static readonly WUIEngineColor Aero = new WUIEngineColor(124, 185, 232, 255);
        private static readonly WUIEngineColor AeroBlue = new WUIEngineColor(201, 255, 229, 255);
        private static readonly WUIEngineColor AfricanViolet = new WUIEngineColor(178, 132, 190, 255);
        private static readonly WUIEngineColor AirForceBlueRAF = new WUIEngineColor(93, 138, 168, 255);
        private static readonly WUIEngineColor AirForceBlueUSAF = new WUIEngineColor(0, 48, 143, 255);
        private static readonly WUIEngineColor AirSuperiorityBlue = new WUIEngineColor(114, 160, 193, 255);
        private static readonly WUIEngineColor AlabamaCrimson = new WUIEngineColor(175, 48, 42, 255);
        private static readonly WUIEngineColor Alabaster = new WUIEngineColor(242, 240, 230, 255);
        private static readonly WUIEngineColor AliceBlue = new WUIEngineColor(240, 248, 255, 255);
        private static readonly WUIEngineColor AlienArmpit = new WUIEngineColor(132, 222, 2, 255);
        private static readonly WUIEngineColor AlizarinCrimson = new WUIEngineColor(227, 38, 54, 255);
        private static readonly WUIEngineColor AlloyOrange = new WUIEngineColor(196, 98, 16, 255);
        private static readonly WUIEngineColor Almond = new WUIEngineColor(239, 222, 205, 255);
        private static readonly WUIEngineColor Amaranth = new WUIEngineColor(229, 43, 80, 255);
        private static readonly WUIEngineColor AmaranthDeepPurple = new WUIEngineColor(159, 43, 104, 255);
        private static readonly WUIEngineColor AmaranthPink = new WUIEngineColor(241, 156, 187, 255);
        private static readonly WUIEngineColor AmaranthPurple = new WUIEngineColor(171, 39, 79, 255);
        private static readonly WUIEngineColor AmaranthRed = new WUIEngineColor(211, 33, 45, 255);
        private static readonly WUIEngineColor Amazon = new WUIEngineColor(59, 122, 87, 255);
        private static readonly WUIEngineColor Amazonite = new WUIEngineColor(0, 196, 176, 255);
        private static readonly WUIEngineColor Amber = new WUIEngineColor(255, 191, 0, 255);
        private static readonly WUIEngineColor AmberSAEECE = new WUIEngineColor(255, 126, 0, 255);
        private static readonly WUIEngineColor AmericanBlue = new WUIEngineColor(59, 59, 109, 255);
        private static readonly WUIEngineColor AmericanBrown = new WUIEngineColor(128, 64, 64, 255);
        private static readonly WUIEngineColor AmericanGold = new WUIEngineColor(211, 175, 55, 255);
        private static readonly WUIEngineColor AmericanGreen = new WUIEngineColor(52, 179, 52, 255);
        private static readonly WUIEngineColor AmericanOrange = new WUIEngineColor(255, 139, 0, 255);
        private static readonly WUIEngineColor AmericanPink = new WUIEngineColor(255, 152, 153, 255);
        private static readonly WUIEngineColor AmericanPurple = new WUIEngineColor(67, 28, 83, 255);
        private static readonly WUIEngineColor AmericanRed = new WUIEngineColor(179, 33, 52, 255);
        private static readonly WUIEngineColor AmericanRose = new WUIEngineColor(255, 3, 62, 255);
        private static readonly WUIEngineColor AmericanSilver = new WUIEngineColor(207, 207, 207, 255);
        private static readonly WUIEngineColor AmericanViolet = new WUIEngineColor(85, 27, 140, 255);
        private static readonly WUIEngineColor AmericanYellow = new WUIEngineColor(242, 180, 0, 255);
        private static readonly WUIEngineColor Amethyst = new WUIEngineColor(153, 102, 204, 255);
        private static readonly WUIEngineColor AndroidGreen = new WUIEngineColor(164, 198, 57, 255);
        private static readonly WUIEngineColor AntiFlashWhite = new WUIEngineColor(242, 243, 244, 255);
        private static readonly WUIEngineColor AntiqueBrass = new WUIEngineColor(205, 149, 117, 255);
        private static readonly WUIEngineColor AntiqueBronze = new WUIEngineColor(102, 93, 30, 255);
        private static readonly WUIEngineColor AntiqueFuchsia = new WUIEngineColor(145, 92, 131, 255);
        private static readonly WUIEngineColor AntiqueRuby = new WUIEngineColor(132, 27, 45, 255);
        private static readonly WUIEngineColor AntiqueWhite = new WUIEngineColor(250, 235, 215, 255);
        private static readonly WUIEngineColor AoEnglish = new WUIEngineColor(0, 128, 0, 255);
        private static readonly WUIEngineColor Apple = new WUIEngineColor(102, 180, 71, 255);
        private static readonly WUIEngineColor AppleGreen = new WUIEngineColor(141, 182, 0, 255);
        private static readonly WUIEngineColor Apricot = new WUIEngineColor(251, 206, 177, 255);
        private static readonly WUIEngineColor Aqua = new WUIEngineColor(0, 255, 255, 255);
        private static readonly WUIEngineColor Aquamarine = new WUIEngineColor(127, 255, 212, 255);
        private static readonly WUIEngineColor ArcticLime = new WUIEngineColor(208, 255, 20, 255);
        private static readonly WUIEngineColor ArmyGreen = new WUIEngineColor(75, 83, 32, 255);
        private static readonly WUIEngineColor Arsenic = new WUIEngineColor(59, 68, 75, 255);
        private static readonly WUIEngineColor Artichoke = new WUIEngineColor(143, 151, 121, 255);
        private static readonly WUIEngineColor ArylideYellow = new WUIEngineColor(233, 214, 107, 255);
        private static readonly WUIEngineColor AshGray = new WUIEngineColor(178, 190, 181, 255);
        private static readonly WUIEngineColor Asparagus = new WUIEngineColor(135, 169, 107, 255);
        private static readonly WUIEngineColor AteneoBlue = new WUIEngineColor(0, 58, 108, 255);
        private static readonly WUIEngineColor AtomicTangerine = new WUIEngineColor(255, 153, 102, 255);
        private static readonly WUIEngineColor Auburn = new WUIEngineColor(165, 42, 42, 255);
        private static readonly WUIEngineColor Aureolin = new WUIEngineColor(253, 238, 0, 255);
        private static readonly WUIEngineColor Aurometalsaurus = new WUIEngineColor(110, 127, 128, 255);
        private static readonly WUIEngineColor Avocado = new WUIEngineColor(86, 130, 3, 255);
        private static readonly WUIEngineColor Awesome = new WUIEngineColor(255, 32, 82, 255);
        private static readonly WUIEngineColor Axolotl = new WUIEngineColor(99, 119, 91, 255);
        private static readonly WUIEngineColor AztecGold = new WUIEngineColor(195, 153, 83, 255);
        private static readonly WUIEngineColor Azure = new WUIEngineColor(0, 127, 255, 255);
        private static readonly WUIEngineColor AzureWebColor = new WUIEngineColor(240, 255, 255, 255);
        private static readonly WUIEngineColor AzureishWhite = new WUIEngineColor(219, 233, 244, 255);
        private static readonly WUIEngineColor BabyBlue = new WUIEngineColor(137, 207, 240, 255);
        private static readonly WUIEngineColor BabyBlueEyes = new WUIEngineColor(161, 202, 241, 255);
        private static readonly WUIEngineColor BabyPink = new WUIEngineColor(244, 194, 194, 255);
        private static readonly WUIEngineColor BabyPowder = new WUIEngineColor(254, 254, 250, 255);
        private static readonly WUIEngineColor BakerMillerPink = new WUIEngineColor(255, 145, 175, 255);
        private static readonly WUIEngineColor BallBlue = new WUIEngineColor(33, 171, 205, 255);
        private static readonly WUIEngineColor BananaMania = new WUIEngineColor(250, 231, 181, 255);
        private static readonly WUIEngineColor BananaYellow = new WUIEngineColor(255, 225, 53, 255);
        private static readonly WUIEngineColor BangladeshGreen = new WUIEngineColor(0, 106, 78, 255);
        private static readonly WUIEngineColor BarbiePink = new WUIEngineColor(224, 33, 138, 255);
        private static readonly WUIEngineColor BarnRed = new WUIEngineColor(124, 10, 2, 255);
        private static readonly WUIEngineColor BatteryChargedBlue = new WUIEngineColor(29, 172, 214, 255);
        private static readonly WUIEngineColor BattleshipGrey = new WUIEngineColor(132, 132, 130, 255);
        private static readonly WUIEngineColor Bazaar = new WUIEngineColor(152, 119, 123, 255);
        private static readonly WUIEngineColor BeauBlue = new WUIEngineColor(188, 212, 230, 255);
        private static readonly WUIEngineColor Beaver = new WUIEngineColor(159, 129, 112, 255);
        private static readonly WUIEngineColor Begonia = new WUIEngineColor(250, 110, 121, 255);
        private static readonly WUIEngineColor Beige = new WUIEngineColor(245, 245, 220, 255);
        private static readonly WUIEngineColor BdazzledBlue = new WUIEngineColor(46, 88, 148, 255);
        private static readonly WUIEngineColor BigDipORuby = new WUIEngineColor(156, 37, 66, 255);
        private static readonly WUIEngineColor BigFootFeet = new WUIEngineColor(232, 142, 90, 255);
        private static readonly WUIEngineColor Bisque = new WUIEngineColor(255, 228, 196, 255);
        private static readonly WUIEngineColor Bistre = new WUIEngineColor(61, 43, 31, 255);
        private static readonly WUIEngineColor BistreBrown = new WUIEngineColor(150, 113, 23, 255);
        private static readonly WUIEngineColor BitterLemon = new WUIEngineColor(202, 224, 13, 255);
        private static readonly WUIEngineColor BitterLime = new WUIEngineColor(191, 255, 0, 255);
        private static readonly WUIEngineColor Bittersweet = new WUIEngineColor(254, 111, 94, 255);
        private static readonly WUIEngineColor BittersweetShimmer = new WUIEngineColor(191, 79, 81, 255);
        private static readonly WUIEngineColor Black = new WUIEngineColor(0, 0, 0, 255);
        private static readonly WUIEngineColor BlackBean = new WUIEngineColor(61, 12, 2, 255);
        private static readonly WUIEngineColor BlackChocolate = new WUIEngineColor(27, 24, 17, 255);
        private static readonly WUIEngineColor BlackCoffee = new WUIEngineColor(59, 47, 47, 255);
        private static readonly WUIEngineColor BlackCoral = new WUIEngineColor(84, 98, 111, 255);
        private static readonly WUIEngineColor BlackLeatherJacket = new WUIEngineColor(37, 53, 41, 255);
        private static readonly WUIEngineColor BlackOlive = new WUIEngineColor(59, 60, 54, 255);
        private static readonly WUIEngineColor Blackberry = new WUIEngineColor(143, 89, 115, 255);
        private static readonly WUIEngineColor BlackShadows = new WUIEngineColor(191, 175, 178, 255);
        private static readonly WUIEngineColor BlanchedAlmond = new WUIEngineColor(255, 235, 205, 255);
        private static readonly WUIEngineColor BlastOffBronze = new WUIEngineColor(165, 113, 100, 255);
        private static readonly WUIEngineColor BleuDeFrance = new WUIEngineColor(49, 140, 231, 255);
        private static readonly WUIEngineColor BlizzardBlue = new WUIEngineColor(172, 229, 238, 255);
        private static readonly WUIEngineColor Blond = new WUIEngineColor(250, 240, 190, 255);
        private static readonly WUIEngineColor BloodOrange = new WUIEngineColor(210, 0, 27, 255);
        private static readonly WUIEngineColor BloodRed = new WUIEngineColor(102, 0, 0, 255);
        private static readonly WUIEngineColor Blue = new WUIEngineColor(0, 0, 255, 255);
        private static readonly WUIEngineColor BlueCrayola = new WUIEngineColor(31, 117, 254, 255);
        private static readonly WUIEngineColor BlueMunsell = new WUIEngineColor(0, 147, 175, 255);
        private static readonly WUIEngineColor BlueNCS = new WUIEngineColor(0, 135, 189, 255);
        private static readonly WUIEngineColor BluePantone = new WUIEngineColor(0, 24, 168, 255);
        private static readonly WUIEngineColor BluePigment = new WUIEngineColor(51, 51, 153, 255);
        private static readonly WUIEngineColor BlueRYB = new WUIEngineColor(2, 71, 254, 255);
        private static readonly WUIEngineColor BlueBell = new WUIEngineColor(162, 162, 208, 255);
        private static readonly WUIEngineColor BlueBolt = new WUIEngineColor(0, 185, 251, 255);
        private static readonly WUIEngineColor BlueGray = new WUIEngineColor(102, 153, 204, 255);
        private static readonly WUIEngineColor BlueGreen = new WUIEngineColor(13, 152, 186, 255);
        private static readonly WUIEngineColor BlueJeans = new WUIEngineColor(93, 173, 236, 255);
        private static readonly WUIEngineColor BlueMagentaViolet = new WUIEngineColor(85, 53, 146, 255);
        private static readonly WUIEngineColor BlueSapphire = new WUIEngineColor(18, 97, 128, 255);
        private static readonly WUIEngineColor BlueViolet = new WUIEngineColor(138, 43, 226, 255);
        private static readonly WUIEngineColor BlueYonder = new WUIEngineColor(80, 114, 167, 255);
        private static readonly WUIEngineColor Blueberry = new WUIEngineColor(79, 134, 247, 255);
        private static readonly WUIEngineColor Bluebonnet = new WUIEngineColor(28, 28, 240, 255);
        private static readonly WUIEngineColor Blush = new WUIEngineColor(222, 93, 131, 255);
        private static readonly WUIEngineColor Bole = new WUIEngineColor(121, 68, 59, 255);
        private static readonly WUIEngineColor BondiBlue = new WUIEngineColor(0, 149, 182, 255);
        private static readonly WUIEngineColor Bone = new WUIEngineColor(227, 218, 201, 255);
        private static readonly WUIEngineColor BoogerBuster = new WUIEngineColor(221, 226, 106, 255);
        private static readonly WUIEngineColor BostonUniversityRed = new WUIEngineColor(204, 0, 0, 255);
        private static readonly WUIEngineColor Boysenberry = new WUIEngineColor(135, 50, 96, 255);
        private static readonly WUIEngineColor BrandeisBlue = new WUIEngineColor(0, 112, 255, 255);
        private static readonly WUIEngineColor Brass = new WUIEngineColor(181, 166, 66, 255);
        private static readonly WUIEngineColor BrickRed = new WUIEngineColor(203, 65, 84, 255);
        private static readonly WUIEngineColor BrightGray = new WUIEngineColor(235, 236, 240, 255);
        private static readonly WUIEngineColor BrightGreen = new WUIEngineColor(102, 255, 0, 255);
        private static readonly WUIEngineColor BrightLavender = new WUIEngineColor(191, 148, 228, 255);
        private static readonly WUIEngineColor BrightLilac = new WUIEngineColor(216, 145, 239, 255);
        private static readonly WUIEngineColor BrightMaroon = new WUIEngineColor(195, 33, 72, 255);
        private static readonly WUIEngineColor BrightNavyBlue = new WUIEngineColor(25, 116, 210, 255);
        private static readonly WUIEngineColor BrightPink = new WUIEngineColor(255, 0, 127, 255);
        private static readonly WUIEngineColor BrightTurquoise = new WUIEngineColor(8, 232, 222, 255);
        private static readonly WUIEngineColor BrightUbe = new WUIEngineColor(209, 159, 232, 255);
        private static readonly WUIEngineColor BrightYellowCrayola = new WUIEngineColor(255, 170, 29, 255);
        private static readonly WUIEngineColor BrilliantAzure = new WUIEngineColor(51, 153, 255, 255);
        private static readonly WUIEngineColor BrilliantLavender = new WUIEngineColor(244, 187, 255, 255);
        private static readonly WUIEngineColor BrilliantRose = new WUIEngineColor(255, 85, 163, 255);
        private static readonly WUIEngineColor BrinkPink = new WUIEngineColor(251, 96, 127, 255);
        private static readonly WUIEngineColor BritishRacingGreen = new WUIEngineColor(0, 66, 37, 255);
        private static readonly WUIEngineColor Bronze = new WUIEngineColor(136, 84, 11, 255);
        private static readonly WUIEngineColor Bronze2 = new WUIEngineColor(205, 127, 50, 255);
        private static readonly WUIEngineColor BronzeMetallic = new WUIEngineColor(176, 140, 86, 255);
        private static readonly WUIEngineColor BronzeYellow = new WUIEngineColor(115, 112, 0, 255);
        private static readonly WUIEngineColor Brown = new WUIEngineColor(153, 51, 0, 255);
        private static readonly WUIEngineColor BrownCrayola = new WUIEngineColor(175, 89, 62, 255);
        private static readonly WUIEngineColor BrownTraditional = new WUIEngineColor(150, 75, 0, 255);
        private static readonly WUIEngineColor BrownNose = new WUIEngineColor(107, 68, 35, 255);
        private static readonly WUIEngineColor BrownSugar = new WUIEngineColor(175, 110, 77, 255);
        private static readonly WUIEngineColor BrownChocolate = new WUIEngineColor(95, 25, 51, 255);
        private static readonly WUIEngineColor BrownCoffee = new WUIEngineColor(74, 44, 42, 255);
        private static readonly WUIEngineColor BrownYellow = new WUIEngineColor(204, 153, 102, 255);
        private static readonly WUIEngineColor BrunswickGreen = new WUIEngineColor(27, 77, 62, 255);
        private static readonly WUIEngineColor BubbleGum = new WUIEngineColor(255, 193, 204, 255);
        private static readonly WUIEngineColor Bubbles = new WUIEngineColor(231, 254, 255, 255);
        private static readonly WUIEngineColor BudGreen = new WUIEngineColor(123, 182, 97, 255);
        private static readonly WUIEngineColor Buff = new WUIEngineColor(240, 220, 130, 255);
        private static readonly WUIEngineColor BulgarianRose = new WUIEngineColor(72, 6, 7, 255);
        private static readonly WUIEngineColor Burgundy = new WUIEngineColor(128, 0, 32, 255);
        private static readonly WUIEngineColor Burlywood = new WUIEngineColor(222, 184, 135, 255);
        private static readonly WUIEngineColor BurnishedBrown = new WUIEngineColor(161, 122, 116, 255);
        private static readonly WUIEngineColor BurntOrange = new WUIEngineColor(204, 85, 0, 255);
        private static readonly WUIEngineColor BurntSienna = new WUIEngineColor(233, 116, 81, 255);
        private static readonly WUIEngineColor BurntUmber = new WUIEngineColor(138, 51, 36, 255);
        private static readonly WUIEngineColor ButtonBlue = new WUIEngineColor(36, 160, 237, 255);
        private static readonly WUIEngineColor Byzantine = new WUIEngineColor(189, 51, 164, 255);
        private static readonly WUIEngineColor Byzantium = new WUIEngineColor(112, 41, 99, 255);
        private static readonly WUIEngineColor Cadet = new WUIEngineColor(83, 104, 114, 255);
        private static readonly WUIEngineColor CadetBlue = new WUIEngineColor(95, 158, 160, 255);
        private static readonly WUIEngineColor CadetGrey = new WUIEngineColor(145, 163, 176, 255);
        private static readonly WUIEngineColor CadmiumBlue = new WUIEngineColor(10, 17, 146, 255);
        private static readonly WUIEngineColor CadmiumGreen = new WUIEngineColor(0, 107, 60, 255);
        private static readonly WUIEngineColor CadmiumOrange = new WUIEngineColor(237, 135, 45, 255);
        private static readonly WUIEngineColor CadmiumPurple = new WUIEngineColor(182, 12, 38, 255);
        private static readonly WUIEngineColor CadmiumRed = new WUIEngineColor(227, 0, 34, 255);
        private static readonly WUIEngineColor CadmiumYellow = new WUIEngineColor(255, 246, 0, 255);
        private static readonly WUIEngineColor CadmiumViolet = new WUIEngineColor(127, 62, 152, 255);
        private static readonly WUIEngineColor CaféAuLait = new WUIEngineColor(166, 123, 91, 255);
        private static readonly WUIEngineColor CaféNoir = new WUIEngineColor(75, 54, 33, 255);
        private static readonly WUIEngineColor CalPolyPomonaGreen = new WUIEngineColor(30, 77, 43, 255);
        private static readonly WUIEngineColor Calamansi = new WUIEngineColor(252, 255, 164, 255);
        private static readonly WUIEngineColor CambridgeBlue = new WUIEngineColor(163, 193, 173, 255);
        private static readonly WUIEngineColor Camel = new WUIEngineColor(193, 154, 107, 255);
        private static readonly WUIEngineColor CameoPink = new WUIEngineColor(239, 187, 204, 255);
        private static readonly WUIEngineColor CamouflageGreen = new WUIEngineColor(120, 134, 107, 255);
        private static readonly WUIEngineColor Canary = new WUIEngineColor(255, 255, 153, 255);
        private static readonly WUIEngineColor CanaryYellow = new WUIEngineColor(255, 239, 0, 255);
        private static readonly WUIEngineColor CandyAppleRed = new WUIEngineColor(255, 8, 0, 255);
        private static readonly WUIEngineColor CandyPink = new WUIEngineColor(228, 113, 122, 255);
        private static readonly WUIEngineColor Capri = new WUIEngineColor(0, 191, 255, 255);
        private static readonly WUIEngineColor CaputMortuum = new WUIEngineColor(89, 39, 32, 255);
        private static readonly WUIEngineColor Caramel = new WUIEngineColor(255, 213, 154, 255);
        private static readonly WUIEngineColor Cardinal = new WUIEngineColor(196, 30, 58, 255);
        private static readonly WUIEngineColor CaribbeanGreen = new WUIEngineColor(0, 204, 153, 255);
        private static readonly WUIEngineColor Carmine = new WUIEngineColor(150, 0, 24, 255);
        private static readonly WUIEngineColor CarmineMP = new WUIEngineColor(215, 0, 64, 255);
        private static readonly WUIEngineColor CarminePink = new WUIEngineColor(235, 76, 66, 255);
        private static readonly WUIEngineColor CarmineRed = new WUIEngineColor(255, 0, 56, 255);
        private static readonly WUIEngineColor CarnationPink = new WUIEngineColor(255, 166, 201, 255);
        private static readonly WUIEngineColor Carnelian = new WUIEngineColor(179, 27, 27, 255);
        private static readonly WUIEngineColor CarolinaBlue = new WUIEngineColor(86, 160, 211, 255);
        private static readonly WUIEngineColor CarrotOrange = new WUIEngineColor(237, 145, 33, 255);
        private static readonly WUIEngineColor CastletonGreen = new WUIEngineColor(0, 86, 63, 255);
        private static readonly WUIEngineColor CatalinaBlue = new WUIEngineColor(6, 42, 120, 255);
        private static readonly WUIEngineColor Catawba = new WUIEngineColor(112, 54, 66, 255);
        private static readonly WUIEngineColor CedarChest = new WUIEngineColor(201, 90, 73, 255);
        private static readonly WUIEngineColor Ceil = new WUIEngineColor(146, 161, 207, 255);
        private static readonly WUIEngineColor Celadon = new WUIEngineColor(172, 225, 175, 255);
        private static readonly WUIEngineColor CeladonBlue = new WUIEngineColor(0, 123, 167, 255);
        private static readonly WUIEngineColor CeladonGreen = new WUIEngineColor(47, 132, 124, 255);
        private static readonly WUIEngineColor Celeste = new WUIEngineColor(178, 255, 255, 255);
        private static readonly WUIEngineColor CelestialBlue = new WUIEngineColor(73, 151, 208, 255);
        private static readonly WUIEngineColor Cerise = new WUIEngineColor(222, 49, 99, 255);
        private static readonly WUIEngineColor CerisePink = new WUIEngineColor(236, 59, 131, 255);
        private static readonly WUIEngineColor CeruleanBlue = new WUIEngineColor(42, 82, 190, 255);
        private static readonly WUIEngineColor CeruleanFrost = new WUIEngineColor(109, 155, 195, 255);
        private static readonly WUIEngineColor CGBlue = new WUIEngineColor(0, 122, 165, 255);
        private static readonly WUIEngineColor CGRed = new WUIEngineColor(224, 60, 49, 255);
        private static readonly WUIEngineColor Chamoisee = new WUIEngineColor(160, 120, 90, 255);
        private static readonly WUIEngineColor Champagne = new WUIEngineColor(247, 231, 206, 255);
        private static readonly WUIEngineColor ChampagnePink = new WUIEngineColor(241, 221, 207, 255);
        private static readonly WUIEngineColor Charcoal = new WUIEngineColor(54, 69, 79, 255);
        private static readonly WUIEngineColor CharlestonGreen = new WUIEngineColor(35, 43, 43, 255);
        private static readonly WUIEngineColor Charm = new WUIEngineColor(208, 116, 139, 255);
        private static readonly WUIEngineColor CharmPink = new WUIEngineColor(230, 143, 172, 255);
        private static readonly WUIEngineColor ChartreuseTraditional = new WUIEngineColor(223, 255, 0, 255);
        private static readonly WUIEngineColor ChartreuseWeb = new WUIEngineColor(127, 255, 0, 255);
        private static readonly WUIEngineColor Cheese = new WUIEngineColor(255, 166, 0, 255);
        private static readonly WUIEngineColor CherryBlossomPink = new WUIEngineColor(255, 183, 197, 255);
        private static readonly WUIEngineColor Chestnut = new WUIEngineColor(149, 69, 53, 255);
        private static readonly WUIEngineColor ChinaPink = new WUIEngineColor(222, 111, 161, 255);
        private static readonly WUIEngineColor ChinaRose = new WUIEngineColor(168, 81, 110, 255);
        private static readonly WUIEngineColor ChineseBlack = new WUIEngineColor(20, 20, 20, 255);
        private static readonly WUIEngineColor ChineseBlue = new WUIEngineColor(54, 81, 148, 255);
        private static readonly WUIEngineColor ChineseBronze = new WUIEngineColor(205, 128, 50, 255);
        private static readonly WUIEngineColor ChineseBrown = new WUIEngineColor(170, 56, 30, 255);
        private static readonly WUIEngineColor ChineseGreen = new WUIEngineColor(208, 219, 97, 255);
        private static readonly WUIEngineColor ChineseGold = new WUIEngineColor(204, 153, 0, 255);
        private static readonly WUIEngineColor ChineseOrange = new WUIEngineColor(243, 112, 66, 255);
        private static readonly WUIEngineColor ChinesePink = new WUIEngineColor(222, 112, 161, 255);
        private static readonly WUIEngineColor ChinesePurple = new WUIEngineColor(114, 11, 152, 255);
        private static readonly WUIEngineColor ChineseRed = new WUIEngineColor(205, 7, 30, 255);
        private static readonly WUIEngineColor ChineseSilver = new WUIEngineColor(204, 204, 204, 255);
        private static readonly WUIEngineColor ChineseViolet = new WUIEngineColor(133, 96, 136, 255);
        private static readonly WUIEngineColor ChineseWhite = new WUIEngineColor(226, 229, 222, 255);
        private static readonly WUIEngineColor ChineseYellow = new WUIEngineColor(255, 178, 0, 255);
        private static readonly WUIEngineColor ChlorophyllGreen = new WUIEngineColor(74, 255, 0, 255);
        private static readonly WUIEngineColor ChocolateKisses = new WUIEngineColor(60, 20, 33, 255);
        private static readonly WUIEngineColor ChocolateTraditional = new WUIEngineColor(123, 63, 0, 255);
        private static readonly WUIEngineColor ChocolateWeb = new WUIEngineColor(210, 105, 30, 255);
        private static readonly WUIEngineColor ChristmasBlue = new WUIEngineColor(42, 143, 189, 255);
        private static readonly WUIEngineColor ChristmasBrown = new WUIEngineColor(93, 43, 44, 255);
        private static readonly WUIEngineColor ChristmasBrown2 = new WUIEngineColor(76, 31, 2, 255);
        private static readonly WUIEngineColor ChristmasGreen = new WUIEngineColor(60, 141, 13, 255);
        private static readonly WUIEngineColor ChristmasGreen2 = new WUIEngineColor(0, 117, 2, 255);
        private static readonly WUIEngineColor ChristmasGold = new WUIEngineColor(202, 169, 6, 255);
        private static readonly WUIEngineColor ChristmasOrange = new WUIEngineColor(255, 102, 0, 255);
        private static readonly WUIEngineColor ChristmasOrange2 = new WUIEngineColor(213, 108, 43, 255);
        private static readonly WUIEngineColor ChristmasPink = new WUIEngineColor(255, 204, 203, 255);
        private static readonly WUIEngineColor ChristmasPink2 = new WUIEngineColor(227, 66, 133, 255);
        private static readonly WUIEngineColor ChristmasPurple = new WUIEngineColor(102, 51, 152, 255);
        private static readonly WUIEngineColor ChristmasPurple2 = new WUIEngineColor(77, 8, 77, 255);
        private static readonly WUIEngineColor ChristmasRed = new WUIEngineColor(170, 1, 20, 255);
        private static readonly WUIEngineColor ChristmasRed2 = new WUIEngineColor(176, 27, 46, 255);
        private static readonly WUIEngineColor ChristmasSilver = new WUIEngineColor(225, 223, 224, 255);
        private static readonly WUIEngineColor ChristmasYellow = new WUIEngineColor(255, 204, 0, 255);
        private static readonly WUIEngineColor ChristmasYellow2 = new WUIEngineColor(254, 242, 0, 255);
        private static readonly WUIEngineColor ChromeYellow = new WUIEngineColor(255, 167, 0, 255);
        private static readonly WUIEngineColor Cinereous = new WUIEngineColor(152, 129, 123, 255);
        private static readonly WUIEngineColor Cinnabar = new WUIEngineColor(227, 66, 52, 255);
        private static readonly WUIEngineColor CinnamonSatin = new WUIEngineColor(205, 96, 126, 255);
        private static readonly WUIEngineColor Citrine = new WUIEngineColor(228, 208, 10, 255);
        private static readonly WUIEngineColor CitrineBrown = new WUIEngineColor(147, 55, 9, 255);
        private static readonly WUIEngineColor Citron = new WUIEngineColor(158, 169, 31, 255);
        private static readonly WUIEngineColor Claret = new WUIEngineColor(127, 23, 52, 255);
        private static readonly WUIEngineColor ClassicRose = new WUIEngineColor(251, 204, 231, 255);
        private static readonly WUIEngineColor CobaltBlue = new WUIEngineColor(0, 71, 171, 255);
        private static readonly WUIEngineColor Coconut = new WUIEngineColor(150, 90, 62, 255);
        private static readonly WUIEngineColor Coffee = new WUIEngineColor(111, 78, 55, 255);
        private static readonly WUIEngineColor Cola = new WUIEngineColor(60, 48, 36, 255);
        private static readonly WUIEngineColor ColumbiaBlue = new WUIEngineColor(196, 216, 226, 255);
        private static readonly WUIEngineColor Conditioner = new WUIEngineColor(255, 255, 204, 255);
        private static readonly WUIEngineColor CongoPink = new WUIEngineColor(248, 131, 121, 255);
        private static readonly WUIEngineColor CoolBlack = new WUIEngineColor(0, 46, 99, 255);
        private static readonly WUIEngineColor CoolGrey = new WUIEngineColor(140, 146, 172, 255);
        private static readonly WUIEngineColor CookiesAndCream = new WUIEngineColor(238, 224, 177, 255);
        private static readonly WUIEngineColor Copper = new WUIEngineColor(184, 115, 51, 255);
        private static readonly WUIEngineColor CopperCrayola = new WUIEngineColor(218, 138, 103, 255);
        private static readonly WUIEngineColor CopperPenny = new WUIEngineColor(173, 111, 105, 255);
        private static readonly WUIEngineColor CopperRed = new WUIEngineColor(203, 109, 81, 255);
        private static readonly WUIEngineColor CopperRose = new WUIEngineColor(153, 102, 102, 255);
        private static readonly WUIEngineColor Coquelicot = new WUIEngineColor(255, 56, 0, 255);
        private static readonly WUIEngineColor Coral = new WUIEngineColor(255, 127, 80, 255);
        private static readonly WUIEngineColor CoralRed = new WUIEngineColor(255, 64, 64, 255);
        private static readonly WUIEngineColor CoralReef = new WUIEngineColor(253, 124, 110, 255);
        private static readonly WUIEngineColor Cordovan = new WUIEngineColor(137, 63, 69, 255);
        private static readonly WUIEngineColor Corn = new WUIEngineColor(251, 236, 93, 255);
        private static readonly WUIEngineColor CornflowerBlue = new WUIEngineColor(100, 149, 237, 255);
        private static readonly WUIEngineColor Cornsilk = new WUIEngineColor(255, 248, 220, 255);
        private static readonly WUIEngineColor CosmicCobalt = new WUIEngineColor(46, 45, 136, 255);
        private static readonly WUIEngineColor CosmicLatte = new WUIEngineColor(255, 248, 231, 255);
        private static readonly WUIEngineColor CoyoteBrown = new WUIEngineColor(129, 97, 60, 255);
        private static readonly WUIEngineColor CottonCandy = new WUIEngineColor(255, 188, 217, 255);
        private static readonly WUIEngineColor Cream = new WUIEngineColor(255, 253, 208, 255);
        private static readonly WUIEngineColor Crimson = new WUIEngineColor(220, 20, 60, 255);
        private static readonly WUIEngineColor CrimsonGlory = new WUIEngineColor(190, 0, 50, 255);
        private static readonly WUIEngineColor CrimsonRed = new WUIEngineColor(153, 0, 0, 255);
        private static readonly WUIEngineColor Cultured = new WUIEngineColor(245, 245, 245, 255);
        private static readonly WUIEngineColor CyanAzure = new WUIEngineColor(78, 130, 180, 255);
        private static readonly WUIEngineColor CyanBlueAzure = new WUIEngineColor(70, 130, 191, 255);
        private static readonly WUIEngineColor CyanCobaltBlue = new WUIEngineColor(40, 88, 156, 255);
        private static readonly WUIEngineColor CyanCornflowerBlue = new WUIEngineColor(24, 139, 194, 255);
        private static readonly WUIEngineColor CyanProcess = new WUIEngineColor(0, 183, 235, 255);
        private static readonly WUIEngineColor CyberGrape = new WUIEngineColor(88, 66, 124, 255);
        private static readonly WUIEngineColor CyberYellow = new WUIEngineColor(255, 211, 0, 255);
        private static readonly WUIEngineColor Cyclamen = new WUIEngineColor(245, 111, 161, 255);
        private static readonly WUIEngineColor Daffodil = new WUIEngineColor(255, 255, 49, 255);
        private static readonly WUIEngineColor Dandelion = new WUIEngineColor(240, 225, 48, 255);
        private static readonly WUIEngineColor DarkBlue = new WUIEngineColor(0, 0, 139, 255);
        private static readonly WUIEngineColor DarkBlueGray = new WUIEngineColor(102, 102, 153, 255);
        private static readonly WUIEngineColor DarkBronze = new WUIEngineColor(128, 74, 0, 255);
        private static readonly WUIEngineColor DarkBrown = new WUIEngineColor(101, 67, 33, 255);
        private static readonly WUIEngineColor DarkBrownTangelo = new WUIEngineColor(136, 101, 78, 255);
        private static readonly WUIEngineColor DarkByzantium = new WUIEngineColor(93, 57, 84, 255);
        private static readonly WUIEngineColor DarkCandyAppleRed = new WUIEngineColor(164, 0, 0, 255);
        private static readonly WUIEngineColor DarkCerulean = new WUIEngineColor(8, 69, 126, 255);
        private static readonly WUIEngineColor DarkCharcoal = new WUIEngineColor(51, 51, 51, 255);
        private static readonly WUIEngineColor DarkChestnut = new WUIEngineColor(152, 105, 96, 255);
        private static readonly WUIEngineColor DarkChocolate = new WUIEngineColor(73, 2, 6, 255);
        private static readonly WUIEngineColor DarkChocolateHersheys = new WUIEngineColor(60, 19, 33, 255);
        private static readonly WUIEngineColor DarkCornflowerBlue = new WUIEngineColor(38, 66, 139, 255);
        private static readonly WUIEngineColor DarkCoral = new WUIEngineColor(205, 91, 69, 255);
        private static readonly WUIEngineColor DarkCyan = new WUIEngineColor(0, 139, 139, 255);
        private static readonly WUIEngineColor DarkElectricBlue = new WUIEngineColor(83, 104, 120, 255);
        private static readonly WUIEngineColor DarkGoldenrod = new WUIEngineColor(184, 134, 11, 255);
        private static readonly WUIEngineColor DarkGrayX11 = new WUIEngineColor(169, 169, 169, 255);
        private static readonly WUIEngineColor DarkGreen = new WUIEngineColor(1, 50, 32, 255);
        private static readonly WUIEngineColor DarkGreenX11 = new WUIEngineColor(0, 100, 0, 255);
        private static readonly WUIEngineColor DarkGunmetal = new WUIEngineColor(31, 38, 42, 255);
        private static readonly WUIEngineColor DarkImperialBlue = new WUIEngineColor(0, 65, 106, 255);
        private static readonly WUIEngineColor DarkImperialBlue2 = new WUIEngineColor(0, 20, 126, 255);
        private static readonly WUIEngineColor DarkJungleGreen = new WUIEngineColor(26, 36, 33, 255);
        private static readonly WUIEngineColor DarkKhaki = new WUIEngineColor(189, 183, 107, 255);
        private static readonly WUIEngineColor DarkLava = new WUIEngineColor(72, 60, 50, 255);
        private static readonly WUIEngineColor DarkLavender = new WUIEngineColor(115, 79, 150, 255);
        private static readonly WUIEngineColor DarkLemonLime = new WUIEngineColor(139, 190, 27, 255);
        private static readonly WUIEngineColor DarkLiver = new WUIEngineColor(83, 75, 79, 255);
        private static readonly WUIEngineColor DarkLiverHorses = new WUIEngineColor(84, 61, 55, 255);
        private static readonly WUIEngineColor DarkMagenta = new WUIEngineColor(139, 0, 139, 255);
        private static readonly WUIEngineColor DarkMidnightBlue = new WUIEngineColor(0, 51, 102, 255);
        private static readonly WUIEngineColor DarkMossGreen = new WUIEngineColor(74, 93, 35, 255);
        private static readonly WUIEngineColor DarkOliveGreen = new WUIEngineColor(85, 107, 47, 255);
        private static readonly WUIEngineColor DarkOrange = new WUIEngineColor(255, 140, 0, 255);
        private static readonly WUIEngineColor DarkOrchid = new WUIEngineColor(153, 50, 204, 255);
        private static readonly WUIEngineColor DarkPastelBlue = new WUIEngineColor(119, 158, 203, 255);
        private static readonly WUIEngineColor DarkPastelGreen = new WUIEngineColor(3, 192, 60, 255);
        private static readonly WUIEngineColor DarkPastelPurple = new WUIEngineColor(150, 111, 214, 255);
        private static readonly WUIEngineColor DarkPastelRed = new WUIEngineColor(194, 59, 34, 255);
        private static readonly WUIEngineColor DarkPink = new WUIEngineColor(231, 84, 128, 255);
        private static readonly WUIEngineColor DarkPowderBlue = new WUIEngineColor(0, 51, 153, 255);
        private static readonly WUIEngineColor DarkPuce = new WUIEngineColor(79, 58, 60, 255);
        private static readonly WUIEngineColor DarkPurple = new WUIEngineColor(48, 25, 52, 255);
        private static readonly WUIEngineColor DarkRaspberry = new WUIEngineColor(135, 38, 87, 255);
        private static readonly WUIEngineColor DarkRed = new WUIEngineColor(139, 0, 0, 255);
        private static readonly WUIEngineColor DarkSalmon = new WUIEngineColor(233, 150, 122, 255);
        private static readonly WUIEngineColor DarkScarlet = new WUIEngineColor(86, 3, 25, 255);
        private static readonly WUIEngineColor DarkSeaGreen = new WUIEngineColor(143, 188, 143, 255);
        private static readonly WUIEngineColor DarkSienna = new WUIEngineColor(60, 20, 20, 255);
        private static readonly WUIEngineColor DarkSkyBlue = new WUIEngineColor(140, 190, 214, 255);
        private static readonly WUIEngineColor DarkSlateBlue = new WUIEngineColor(72, 61, 139, 255);
        private static readonly WUIEngineColor DarkSlateGray = new WUIEngineColor(47, 79, 79, 255);
        private static readonly WUIEngineColor DarkSpringGreen = new WUIEngineColor(23, 114, 69, 255);
        private static readonly WUIEngineColor DarkTan = new WUIEngineColor(145, 129, 81, 255);
        private static readonly WUIEngineColor DarkTangerine = new WUIEngineColor(255, 168, 18, 255);
        private static readonly WUIEngineColor DarkTerraCotta = new WUIEngineColor(204, 78, 92, 255);
        private static readonly WUIEngineColor DarkTurquoise = new WUIEngineColor(0, 206, 209, 255);
        private static readonly WUIEngineColor DarkVanilla = new WUIEngineColor(209, 190, 168, 255);
        private static readonly WUIEngineColor DarkViolet = new WUIEngineColor(148, 0, 211, 255);
        private static readonly WUIEngineColor DarkYellow = new WUIEngineColor(155, 135, 12, 255);
        private static readonly WUIEngineColor DartmouthGreen = new WUIEngineColor(0, 112, 60, 255);
        private static readonly WUIEngineColor DavysGrey = new WUIEngineColor(85, 85, 85, 255);
        private static readonly WUIEngineColor DebianRed = new WUIEngineColor(215, 10, 83, 255);
        private static readonly WUIEngineColor DeepAmethyst = new WUIEngineColor(156, 138, 164, 255);
        private static readonly WUIEngineColor DeepAquamarine = new WUIEngineColor(64, 130, 109, 255);
        private static readonly WUIEngineColor DeepCarmine = new WUIEngineColor(169, 32, 62, 255);
        private static readonly WUIEngineColor DeepCarminePink = new WUIEngineColor(239, 48, 56, 255);
        private static readonly WUIEngineColor DeepCarrotOrange = new WUIEngineColor(233, 105, 44, 255);
        private static readonly WUIEngineColor DeepCerise = new WUIEngineColor(218, 50, 135, 255);
        private static readonly WUIEngineColor DeepChampagne = new WUIEngineColor(250, 214, 165, 255);
        private static readonly WUIEngineColor DeepChestnut = new WUIEngineColor(185, 78, 72, 255);
        private static readonly WUIEngineColor DeepCoffee = new WUIEngineColor(112, 66, 65, 255);
        private static readonly WUIEngineColor DeepFuchsia = new WUIEngineColor(193, 84, 193, 255);
        private static readonly WUIEngineColor DeepGreen = new WUIEngineColor(5, 102, 8, 255);
        private static readonly WUIEngineColor DeepGreenCyanTurquoise = new WUIEngineColor(14, 124, 97, 255);
        private static readonly WUIEngineColor DeepJungleGreen = new WUIEngineColor(0, 75, 73, 255);
        private static readonly WUIEngineColor DeepKoamaru = new WUIEngineColor(51, 51, 102, 255);
        private static readonly WUIEngineColor DeepLemon = new WUIEngineColor(245, 199, 26, 255);
        private static readonly WUIEngineColor DeepLilac = new WUIEngineColor(153, 85, 187, 255);
        private static readonly WUIEngineColor DeepMagenta = new WUIEngineColor(204, 0, 204, 255);
        private static readonly WUIEngineColor DeepMaroon = new WUIEngineColor(130, 0, 0, 255);
        private static readonly WUIEngineColor DeepMauve = new WUIEngineColor(212, 115, 212, 255);
        private static readonly WUIEngineColor DeepMossGreen = new WUIEngineColor(53, 94, 59, 255);
        private static readonly WUIEngineColor DeepPeach = new WUIEngineColor(255, 203, 164, 255);
        private static readonly WUIEngineColor DeepPink = new WUIEngineColor(255, 20, 147, 255);
        private static readonly WUIEngineColor DeepPuce = new WUIEngineColor(169, 92, 104, 255);
        private static readonly WUIEngineColor DeepRed = new WUIEngineColor(133, 1, 1, 255);
        private static readonly WUIEngineColor DeepRuby = new WUIEngineColor(132, 63, 91, 255);
        private static readonly WUIEngineColor DeepSaffron = new WUIEngineColor(255, 153, 51, 255);
        private static readonly WUIEngineColor DeepSpaceSparkle = new WUIEngineColor(74, 100, 108, 255);
        private static readonly WUIEngineColor DeepTaupe = new WUIEngineColor(126, 94, 96, 255);
        private static readonly WUIEngineColor DeepTuscanRed = new WUIEngineColor(102, 66, 77, 255);
        private static readonly WUIEngineColor DeepViolet = new WUIEngineColor(51, 0, 102, 255);
        private static readonly WUIEngineColor Deer = new WUIEngineColor(186, 135, 89, 255);
        private static readonly WUIEngineColor Denim = new WUIEngineColor(21, 96, 189, 255);
        private static readonly WUIEngineColor DenimBlue = new WUIEngineColor(34, 67, 182, 255);
        private static readonly WUIEngineColor DesaturatedCyan = new WUIEngineColor(102, 153, 153, 255);
        private static readonly WUIEngineColor DesertSand = new WUIEngineColor(237, 201, 175, 255);
        private static readonly WUIEngineColor Desire = new WUIEngineColor(234, 60, 83, 255);
        private static readonly WUIEngineColor Diamond = new WUIEngineColor(185, 242, 255, 255);
        private static readonly WUIEngineColor DimGray = new WUIEngineColor(105, 105, 105, 255);
        private static readonly WUIEngineColor DingyDungeon = new WUIEngineColor(197, 49, 81, 255);
        private static readonly WUIEngineColor Dirt = new WUIEngineColor(155, 118, 83, 255);
        private static readonly WUIEngineColor DirtyBrown = new WUIEngineColor(181, 101, 30, 255);
        private static readonly WUIEngineColor DirtyWhite = new WUIEngineColor(232, 228, 201, 255);
        private static readonly WUIEngineColor DodgerBlue = new WUIEngineColor(30, 144, 255, 255);
        private static readonly WUIEngineColor DodieYellow = new WUIEngineColor(254, 246, 91, 255);
        private static readonly WUIEngineColor DogwoodRose = new WUIEngineColor(215, 24, 104, 255);
        private static readonly WUIEngineColor DollarBill = new WUIEngineColor(133, 187, 101, 255);
        private static readonly WUIEngineColor DolphinGray = new WUIEngineColor(130, 142, 132, 255);
        private static readonly WUIEngineColor DonkeyBrown = new WUIEngineColor(102, 76, 40, 255);
        private static readonly WUIEngineColor DukeBlue = new WUIEngineColor(0, 0, 156, 255);
        private static readonly WUIEngineColor DustStorm = new WUIEngineColor(229, 204, 201, 255);
        private static readonly WUIEngineColor DutchWhite = new WUIEngineColor(239, 223, 187, 255);
        private static readonly WUIEngineColor EarthYellow = new WUIEngineColor(225, 169, 95, 255);
        private static readonly WUIEngineColor Ebony = new WUIEngineColor(85, 93, 80, 255);
        private static readonly WUIEngineColor Ecru = new WUIEngineColor(194, 178, 128, 255);
        private static readonly WUIEngineColor EerieBlack = new WUIEngineColor(27, 27, 27, 255);
        private static readonly WUIEngineColor Eggplant = new WUIEngineColor(97, 64, 81, 255);
        private static readonly WUIEngineColor Eggshell = new WUIEngineColor(240, 234, 214, 255);
        private static readonly WUIEngineColor EgyptianBlue = new WUIEngineColor(16, 52, 166, 255);
        private static readonly WUIEngineColor ElectricBlue = new WUIEngineColor(125, 249, 255, 255);
        private static readonly WUIEngineColor ElectricCrimson = new WUIEngineColor(255, 0, 63, 255);
        private static readonly WUIEngineColor ElectricGreen = new WUIEngineColor(0, 255, 0, 255);
        private static readonly WUIEngineColor ElectricIndigo = new WUIEngineColor(111, 0, 255, 255);
        private static readonly WUIEngineColor ElectricLime = new WUIEngineColor(204, 255, 0, 255);
        private static readonly WUIEngineColor ElectricPurple = new WUIEngineColor(191, 0, 255, 255);
        private static readonly WUIEngineColor ElectricUltramarine = new WUIEngineColor(63, 0, 255, 255);
        private static readonly WUIEngineColor ElectricViolet = new WUIEngineColor(143, 0, 255, 255);
        private static readonly WUIEngineColor ElectricYellow = new WUIEngineColor(255, 255, 51, 255);
        private static readonly WUIEngineColor Emerald = new WUIEngineColor(80, 200, 120, 255);
        private static readonly WUIEngineColor EmeraldGreen = new WUIEngineColor(4, 99, 7, 255);
        private static readonly WUIEngineColor Eminence = new WUIEngineColor(108, 48, 130, 255);
        private static readonly WUIEngineColor EnglishLavender = new WUIEngineColor(180, 131, 149, 255);
        private static readonly WUIEngineColor EnglishRed = new WUIEngineColor(171, 75, 82, 255);
        private static readonly WUIEngineColor EnglishVermillion = new WUIEngineColor(204, 71, 75, 255);
        private static readonly WUIEngineColor EnglishViolet = new WUIEngineColor(86, 60, 92, 255);
        private static readonly WUIEngineColor EtonBlue = new WUIEngineColor(150, 200, 162, 255);
        private static readonly WUIEngineColor Eucalyptus = new WUIEngineColor(68, 215, 168, 255);
        private static readonly WUIEngineColor FaluRed = new WUIEngineColor(128, 24, 24, 255);
        private static readonly WUIEngineColor Fandango = new WUIEngineColor(181, 51, 137, 255);
        private static readonly WUIEngineColor FandangoPink = new WUIEngineColor(222, 82, 133, 255);
        private static readonly WUIEngineColor FashionFuchsia = new WUIEngineColor(244, 0, 161, 255);
        private static readonly WUIEngineColor Fawn = new WUIEngineColor(229, 170, 112, 255);
        private static readonly WUIEngineColor Feldgrau = new WUIEngineColor(77, 93, 83, 255);
        private static readonly WUIEngineColor Feldspar = new WUIEngineColor(253, 213, 177, 255);
        private static readonly WUIEngineColor FernGreen = new WUIEngineColor(79, 121, 66, 255);
        private static readonly WUIEngineColor FerrariRed = new WUIEngineColor(255, 40, 0, 255);
        private static readonly WUIEngineColor FieldDrab = new WUIEngineColor(108, 84, 30, 255);
        private static readonly WUIEngineColor FieryRose = new WUIEngineColor(255, 84, 112, 255);
        private static readonly WUIEngineColor Firebrick = new WUIEngineColor(178, 34, 34, 255);
        private static readonly WUIEngineColor FireEngineRed = new WUIEngineColor(206, 32, 41, 255);
        private static readonly WUIEngineColor FireOpal = new WUIEngineColor(233, 92, 75, 255);
        private static readonly WUIEngineColor Flame = new WUIEngineColor(226, 88, 34, 255);
        private static readonly WUIEngineColor FlamingoPink = new WUIEngineColor(252, 142, 172, 255);
        private static readonly WUIEngineColor Flavescent = new WUIEngineColor(247, 233, 142, 255);
        private static readonly WUIEngineColor Flax = new WUIEngineColor(238, 220, 130, 255);
        private static readonly WUIEngineColor Flesh = new WUIEngineColor(255, 233, 209, 255);
        private static readonly WUIEngineColor Flirt = new WUIEngineColor(162, 0, 109, 255);
        private static readonly WUIEngineColor FloralWhite = new WUIEngineColor(255, 250, 240, 255);
        private static readonly WUIEngineColor Folly = new WUIEngineColor(255, 0, 79, 255);
        private static readonly WUIEngineColor ForestGreenTraditional = new WUIEngineColor(1, 68, 33, 255);
        private static readonly WUIEngineColor ForestGreenWeb = new WUIEngineColor(34, 139, 34, 255);
        private static readonly WUIEngineColor FrenchBistre = new WUIEngineColor(133, 109, 77, 255);
        private static readonly WUIEngineColor FrenchBlue = new WUIEngineColor(0, 114, 187, 255);
        private static readonly WUIEngineColor FrenchFuchsia = new WUIEngineColor(253, 63, 146, 255);
        private static readonly WUIEngineColor FrenchLilac = new WUIEngineColor(134, 96, 142, 255);
        private static readonly WUIEngineColor FrenchLime = new WUIEngineColor(158, 253, 56, 255);
        private static readonly WUIEngineColor FrenchPink = new WUIEngineColor(253, 108, 158, 255);
        private static readonly WUIEngineColor FrenchPlum = new WUIEngineColor(129, 20, 83, 255);
        private static readonly WUIEngineColor FrenchPuce = new WUIEngineColor(78, 22, 9, 255);
        private static readonly WUIEngineColor FrenchRaspberry = new WUIEngineColor(199, 44, 72, 255);
        private static readonly WUIEngineColor FrenchRose = new WUIEngineColor(246, 74, 138, 255);
        private static readonly WUIEngineColor FrenchSkyBlue = new WUIEngineColor(119, 181, 254, 255);
        private static readonly WUIEngineColor FrenchViolet = new WUIEngineColor(136, 6, 206, 255);
        private static readonly WUIEngineColor FrenchWine = new WUIEngineColor(172, 30, 68, 255);
        private static readonly WUIEngineColor FreshAir = new WUIEngineColor(166, 231, 255, 255);
        private static readonly WUIEngineColor Frostbite = new WUIEngineColor(233, 54, 167, 255);
        private static readonly WUIEngineColor Fuchsia = new WUIEngineColor(255, 0, 255, 255);
        private static readonly WUIEngineColor FuchsiaPink = new WUIEngineColor(255, 119, 255, 255);
        private static readonly WUIEngineColor FuchsiaPurple = new WUIEngineColor(204, 57, 123, 255);
        private static readonly WUIEngineColor FuchsiaRose = new WUIEngineColor(199, 67, 117, 255);
        private static readonly WUIEngineColor Fulvous = new WUIEngineColor(228, 132, 0, 255);
        private static readonly WUIEngineColor FuzzyWuzzy = new WUIEngineColor(204, 102, 102, 255);
        private static readonly WUIEngineColor Gainsboro = new WUIEngineColor(220, 220, 220, 255);
        private static readonly WUIEngineColor Gamboge = new WUIEngineColor(228, 155, 15, 255);
        private static readonly WUIEngineColor GambogeOrangeBrown = new WUIEngineColor(152, 102, 0, 255);
        private static readonly WUIEngineColor Garnet = new WUIEngineColor(115, 54, 53, 255);
        private static readonly WUIEngineColor GargoyleGas = new WUIEngineColor(255, 223, 70, 255);
        private static readonly WUIEngineColor GenericViridian = new WUIEngineColor(0, 127, 102, 255);
        private static readonly WUIEngineColor GhostWhite = new WUIEngineColor(248, 248, 255, 255);
        private static readonly WUIEngineColor GiantsClub = new WUIEngineColor(176, 92, 82, 255);
        private static readonly WUIEngineColor GiantsOrange = new WUIEngineColor(254, 90, 29, 255);
        private static readonly WUIEngineColor Glaucous = new WUIEngineColor(96, 130, 182, 255);
        private static readonly WUIEngineColor GlossyGrape = new WUIEngineColor(171, 146, 179, 255);
        private static readonly WUIEngineColor GOGreen = new WUIEngineColor(0, 171, 102, 255);
        private static readonly WUIEngineColor Gold = new WUIEngineColor(165, 124, 0, 255);
        private static readonly WUIEngineColor GoldMetallic = new WUIEngineColor(212, 175, 55, 255);
        private static readonly WUIEngineColor GoldWebGolden = new WUIEngineColor(255, 215, 0, 255);
        private static readonly WUIEngineColor GoldCrayola = new WUIEngineColor(230, 190, 138, 255);
        private static readonly WUIEngineColor GoldFusion = new WUIEngineColor(133, 117, 78, 255);
        private static readonly WUIEngineColor GoldFoil = new WUIEngineColor(189, 155, 22, 255);
        private static readonly WUIEngineColor GoldenBrown = new WUIEngineColor(153, 101, 21, 255);
        private static readonly WUIEngineColor GoldenPoppy = new WUIEngineColor(252, 194, 0, 255);
        private static readonly WUIEngineColor GoldenYellow = new WUIEngineColor(255, 223, 0, 255);
        private static readonly WUIEngineColor Goldenrod = new WUIEngineColor(218, 165, 32, 255);
        private static readonly WUIEngineColor GraniteGray = new WUIEngineColor(103, 103, 103, 255);
        private static readonly WUIEngineColor GrannySmithApple = new WUIEngineColor(168, 228, 160, 255);
        private static readonly WUIEngineColor Grape = new WUIEngineColor(111, 45, 168, 255);
        private static readonly WUIEngineColor GrayHTMLCSSGray = new WUIEngineColor(128, 128, 128, 255);
        private static readonly WUIEngineColor GrayX11Gray = new WUIEngineColor(190, 190, 190, 255);
        private static readonly WUIEngineColor GrayAsparagus = new WUIEngineColor(70, 89, 69, 255);
        private static readonly WUIEngineColor Green = new WUIEngineColor(0, 128, 1, 255);
        private static readonly WUIEngineColor GreenCrayola = new WUIEngineColor(28, 172, 120, 255);
        private static readonly WUIEngineColor GreenMunsell = new WUIEngineColor(0, 168, 119, 255);
        private static readonly WUIEngineColor GreenNCS = new WUIEngineColor(0, 159, 107, 255);
        private static readonly WUIEngineColor GreenPantone = new WUIEngineColor(0, 173, 67, 255);
        private static readonly WUIEngineColor GreenPigment = new WUIEngineColor(0, 165, 80, 255);
        private static readonly WUIEngineColor GreenRYB = new WUIEngineColor(102, 176, 50, 255);
        private static readonly WUIEngineColor GreenBlue = new WUIEngineColor(17, 100, 180, 255);
        private static readonly WUIEngineColor GreenCyan = new WUIEngineColor(0, 153, 102, 255);
        private static readonly WUIEngineColor GreenLizard = new WUIEngineColor(167, 244, 50, 255);
        private static readonly WUIEngineColor GreenSheen = new WUIEngineColor(110, 174, 161, 255);
        private static readonly WUIEngineColor GreenYellow = new WUIEngineColor(173, 255, 47, 255);
        private static readonly WUIEngineColor Grullo = new WUIEngineColor(169, 154, 134, 255);
        private static readonly WUIEngineColor GuppieGreen = new WUIEngineColor(0, 255, 127, 255);
        private static readonly WUIEngineColor Gunmetal = new WUIEngineColor(42, 52, 57, 255);
        private static readonly WUIEngineColor HalayàÚbe = new WUIEngineColor(102, 55, 84, 255);
        private static readonly WUIEngineColor HalloweenOrange = new WUIEngineColor(235, 97, 35, 255);
        private static readonly WUIEngineColor HanBlue = new WUIEngineColor(68, 108, 207, 255);
        private static readonly WUIEngineColor HanPurple = new WUIEngineColor(82, 24, 250, 255);
        private static readonly WUIEngineColor Harlequin = new WUIEngineColor(63, 255, 0, 255);
        private static readonly WUIEngineColor HarlequinGreen = new WUIEngineColor(70, 203, 24, 255);
        private static readonly WUIEngineColor HarvardCrimson = new WUIEngineColor(201, 0, 22, 255);
        private static readonly WUIEngineColor HarvestGold = new WUIEngineColor(218, 145, 0, 255);
        private static readonly WUIEngineColor HeartGold = new WUIEngineColor(128, 128, 0, 255);
        private static readonly WUIEngineColor HeatWave = new WUIEngineColor(255, 122, 0, 255);
        private static readonly WUIEngineColor Heliotrope = new WUIEngineColor(223, 115, 255, 255);
        private static readonly WUIEngineColor HeliotropeGray = new WUIEngineColor(170, 152, 168, 255);
        private static readonly WUIEngineColor HeliotropeMagenta = new WUIEngineColor(170, 0, 187, 255);
        private static readonly WUIEngineColor Honeydew = new WUIEngineColor(240, 255, 240, 255);
        private static readonly WUIEngineColor HonoluluBlue = new WUIEngineColor(0, 109, 176, 255);
        private static readonly WUIEngineColor HookersGreen = new WUIEngineColor(73, 121, 107, 255);
        private static readonly WUIEngineColor HotMagenta = new WUIEngineColor(255, 29, 206, 255);
        private static readonly WUIEngineColor HotPink = new WUIEngineColor(255, 105, 180, 255);
        private static readonly WUIEngineColor Iceberg = new WUIEngineColor(113, 166, 210, 255);
        private static readonly WUIEngineColor Icterine = new WUIEngineColor(252, 247, 94, 255);
        private static readonly WUIEngineColor IguanaGreen = new WUIEngineColor(113, 188, 120, 255);
        private static readonly WUIEngineColor IlluminatingEmerald = new WUIEngineColor(49, 145, 119, 255);
        private static readonly WUIEngineColor Imperial = new WUIEngineColor(96, 47, 107, 255);
        private static readonly WUIEngineColor ImperialBlue = new WUIEngineColor(0, 35, 149, 255);
        private static readonly WUIEngineColor ImperialPurple = new WUIEngineColor(102, 2, 60, 255);
        private static readonly WUIEngineColor ImperialRed = new WUIEngineColor(237, 41, 57, 255);
        private static readonly WUIEngineColor Inchworm = new WUIEngineColor(178, 236, 93, 255);
        private static readonly WUIEngineColor Independence = new WUIEngineColor(76, 81, 109, 255);
        private static readonly WUIEngineColor IndiaGreen = new WUIEngineColor(19, 136, 8, 255);
        private static readonly WUIEngineColor IndianRed = new WUIEngineColor(205, 92, 92, 255);
        private static readonly WUIEngineColor IndianYellow = new WUIEngineColor(227, 168, 87, 255);
        private static readonly WUIEngineColor Indigo = new WUIEngineColor(75, 0, 130, 255);
        private static readonly WUIEngineColor IndigoDye = new WUIEngineColor(9, 31, 146, 255);
        private static readonly WUIEngineColor IndigoRainbow = new WUIEngineColor(35, 48, 103, 255);
        private static readonly WUIEngineColor InfraRed = new WUIEngineColor(255, 73, 108, 255);
        private static readonly WUIEngineColor InterdimensionalBlue = new WUIEngineColor(54, 12, 204, 255);
        private static readonly WUIEngineColor InternationalKleinBlue = new WUIEngineColor(0, 47, 167, 255);
        private static readonly WUIEngineColor InternationalOrangeAerospace = new WUIEngineColor(255, 79, 0, 255);
        private static readonly WUIEngineColor InternationalOrangeEngineering = new WUIEngineColor(186, 22, 12, 255);
        private static readonly WUIEngineColor InternationalOrangeGoldenGateBridge = new WUIEngineColor(192, 54, 44, 255);
        private static readonly WUIEngineColor Iris = new WUIEngineColor(90, 79, 207, 255);
        private static readonly WUIEngineColor Irresistible = new WUIEngineColor(179, 68, 108, 255);
        private static readonly WUIEngineColor Isabelline = new WUIEngineColor(244, 240, 236, 255);
        private static readonly WUIEngineColor IslamicGreen = new WUIEngineColor(0, 144, 0, 255);
        private static readonly WUIEngineColor Ivory = new WUIEngineColor(255, 255, 240, 255);
        private static readonly WUIEngineColor Jacarta = new WUIEngineColor(61, 50, 93, 255);
        private static readonly WUIEngineColor JackoBean = new WUIEngineColor(65, 54, 40, 255);
        private static readonly WUIEngineColor Jade = new WUIEngineColor(0, 168, 107, 255);
        private static readonly WUIEngineColor JapaneseCarmine = new WUIEngineColor(157, 41, 51, 255);
        private static readonly WUIEngineColor JapaneseIndigo = new WUIEngineColor(38, 67, 72, 255);
        private static readonly WUIEngineColor JapaneseLaurel = new WUIEngineColor(47, 117, 50, 255);
        private static readonly WUIEngineColor JapaneseViolet = new WUIEngineColor(91, 50, 86, 255);
        private static readonly WUIEngineColor Jasmine = new WUIEngineColor(248, 222, 126, 255);
        private static readonly WUIEngineColor Jasper = new WUIEngineColor(215, 59, 62, 255);
        private static readonly WUIEngineColor JasperOrange = new WUIEngineColor(223, 145, 79, 255);
        private static readonly WUIEngineColor JazzberryJam = new WUIEngineColor(165, 11, 94, 255);
        private static readonly WUIEngineColor JellyBean = new WUIEngineColor(218, 97, 78, 255);
        private static readonly WUIEngineColor JellyBeanBlue = new WUIEngineColor(68, 121, 142, 255);
        private static readonly WUIEngineColor Jet = new WUIEngineColor(52, 52, 52, 255);
        private static readonly WUIEngineColor JetStream = new WUIEngineColor(187, 208, 201, 255);
        private static readonly WUIEngineColor Jonquil = new WUIEngineColor(244, 202, 22, 255);
        private static readonly WUIEngineColor JordyBlue = new WUIEngineColor(138, 185, 241, 255);
        private static readonly WUIEngineColor JuneBud = new WUIEngineColor(189, 218, 87, 255);
        private static readonly WUIEngineColor JungleGreen = new WUIEngineColor(41, 171, 135, 255);
        private static readonly WUIEngineColor KellyGreen = new WUIEngineColor(76, 187, 23, 255);
        private static readonly WUIEngineColor KenyanCopper = new WUIEngineColor(124, 28, 5, 255);
        private static readonly WUIEngineColor Keppel = new WUIEngineColor(58, 176, 158, 255);
        private static readonly WUIEngineColor KeyLime = new WUIEngineColor(232, 244, 140, 255);
        private static readonly WUIEngineColor KhakiHTMLCSSKhaki = new WUIEngineColor(195, 176, 145, 255);
        private static readonly WUIEngineColor KhakiX11LightKhaki = new WUIEngineColor(240, 230, 140, 255);
        private static readonly WUIEngineColor Kiwi = new WUIEngineColor(142, 229, 63, 255);
        private static readonly WUIEngineColor Kobe = new WUIEngineColor(136, 45, 23, 255);
        private static readonly WUIEngineColor Kobi = new WUIEngineColor(231, 159, 196, 255);
        private static readonly WUIEngineColor KombuGreen = new WUIEngineColor(53, 66, 48, 255);
        private static readonly WUIEngineColor KSUPurple = new WUIEngineColor(79, 38, 131, 255);
        private static readonly WUIEngineColor KUCrimson = new WUIEngineColor(232, 0, 13, 255);
        private static readonly WUIEngineColor LaSalleGreen = new WUIEngineColor(8, 120, 48, 255);
        private static readonly WUIEngineColor LanguidLavender = new WUIEngineColor(214, 202, 221, 255);
        private static readonly WUIEngineColor LapisLazuli = new WUIEngineColor(38, 97, 156, 255);
        private static readonly WUIEngineColor LaserLemon = new WUIEngineColor(255, 255, 102, 255);
        private static readonly WUIEngineColor LaurelGreen = new WUIEngineColor(169, 186, 157, 255);
        private static readonly WUIEngineColor Lava = new WUIEngineColor(207, 16, 32, 255);
        private static readonly WUIEngineColor LavenderFloral = new WUIEngineColor(181, 126, 220, 255);
        private static readonly WUIEngineColor LavenderWeb = new WUIEngineColor(230, 230, 250, 255);
        private static readonly WUIEngineColor LavenderBlue = new WUIEngineColor(204, 204, 255, 255);
        private static readonly WUIEngineColor LavenderBlush = new WUIEngineColor(255, 240, 245, 255);
        private static readonly WUIEngineColor LavenderGray = new WUIEngineColor(196, 195, 208, 255);
        private static readonly WUIEngineColor LavenderIndigo = new WUIEngineColor(148, 87, 235, 255);
        private static readonly WUIEngineColor LavenderMagenta = new WUIEngineColor(238, 130, 238, 255);
        private static readonly WUIEngineColor LavenderPink = new WUIEngineColor(251, 174, 210, 255);
        private static readonly WUIEngineColor LavenderPurple = new WUIEngineColor(150, 123, 182, 255);
        private static readonly WUIEngineColor LavenderRose = new WUIEngineColor(251, 160, 227, 255);
        private static readonly WUIEngineColor LawnGreen = new WUIEngineColor(124, 252, 0, 255);
        private static readonly WUIEngineColor Lemon = new WUIEngineColor(255, 247, 0, 255);
        private static readonly WUIEngineColor LemonChiffon = new WUIEngineColor(255, 250, 205, 255);
        private static readonly WUIEngineColor LemonCurry = new WUIEngineColor(204, 160, 29, 255);
        private static readonly WUIEngineColor LemonGlacier = new WUIEngineColor(253, 255, 0, 255);
        private static readonly WUIEngineColor LemonMeringue = new WUIEngineColor(246, 234, 190, 255);
        private static readonly WUIEngineColor LemonYellow = new WUIEngineColor(255, 244, 79, 255);
        private static readonly WUIEngineColor LemonYellowCrayola = new WUIEngineColor(255, 255, 159, 255);
        private static readonly WUIEngineColor Lenurple = new WUIEngineColor(186, 147, 216, 255);
        private static readonly WUIEngineColor Liberty = new WUIEngineColor(84, 90, 167, 255);
        private static readonly WUIEngineColor Licorice = new WUIEngineColor(26, 17, 16, 255);
        private static readonly WUIEngineColor LightBlue = new WUIEngineColor(173, 216, 230, 255);
        private static readonly WUIEngineColor LightBrown = new WUIEngineColor(181, 101, 29, 255);
        private static readonly WUIEngineColor LightCarminePink = new WUIEngineColor(230, 103, 113, 255);
        private static readonly WUIEngineColor LightCobaltBlue = new WUIEngineColor(136, 172, 224, 255);
        private static readonly WUIEngineColor LightCoral = new WUIEngineColor(240, 128, 128, 255);
        private static readonly WUIEngineColor LightCornflowerBlue = new WUIEngineColor(147, 204, 234, 255);
        private static readonly WUIEngineColor LightCrimson = new WUIEngineColor(245, 105, 145, 255);
        private static readonly WUIEngineColor LightCyan = new WUIEngineColor(224, 255, 255, 255);
        private static readonly WUIEngineColor LightDeepPink = new WUIEngineColor(255, 92, 205, 255);
        private static readonly WUIEngineColor LightFrenchBeige = new WUIEngineColor(200, 173, 127, 255);
        private static readonly WUIEngineColor LightFuchsiaPink = new WUIEngineColor(249, 132, 239, 255);
        private static readonly WUIEngineColor LightGold = new WUIEngineColor(178, 151, 0, 255);
        private static readonly WUIEngineColor LightGoldenrodYellow = new WUIEngineColor(250, 250, 210, 255);
        private static readonly WUIEngineColor LightGray = new WUIEngineColor(211, 211, 211, 255);
        private static readonly WUIEngineColor LightGrayishMagenta = new WUIEngineColor(204, 153, 204, 255);
        private static readonly WUIEngineColor LightGreen = new WUIEngineColor(144, 238, 144, 255);
        private static readonly WUIEngineColor LightHotPink = new WUIEngineColor(255, 179, 222, 255);
        private static readonly WUIEngineColor LightMediumOrchid = new WUIEngineColor(211, 155, 203, 255);
        private static readonly WUIEngineColor LightMossGreen = new WUIEngineColor(173, 223, 173, 255);
        private static readonly WUIEngineColor LightOrange = new WUIEngineColor(254, 216, 177, 255);
        private static readonly WUIEngineColor LightOrchid = new WUIEngineColor(230, 168, 215, 255);
        private static readonly WUIEngineColor LightPastelPurple = new WUIEngineColor(177, 156, 217, 255);
        private static readonly WUIEngineColor LightPeriwinkle = new WUIEngineColor(197, 203, 225, 255);
        private static readonly WUIEngineColor LightPink = new WUIEngineColor(255, 182, 193, 255);
        private static readonly WUIEngineColor LightSalmon = new WUIEngineColor(255, 160, 122, 255);
        private static readonly WUIEngineColor LightSalmonPink = new WUIEngineColor(255, 153, 153, 255);
        private static readonly WUIEngineColor LightSeaGreen = new WUIEngineColor(32, 178, 170, 255);
        private static readonly WUIEngineColor LightSilver = new WUIEngineColor(216, 216, 216, 255);
        private static readonly WUIEngineColor LightSkyBlue = new WUIEngineColor(135, 206, 250, 255);
        private static readonly WUIEngineColor LightSlateGray = new WUIEngineColor(119, 136, 153, 255);
        private static readonly WUIEngineColor LightSteelBlue = new WUIEngineColor(176, 196, 222, 255);
        private static readonly WUIEngineColor LightTaupe = new WUIEngineColor(179, 139, 109, 255);
        private static readonly WUIEngineColor LightYellow = new WUIEngineColor(255, 255, 224, 255);
        private static readonly WUIEngineColor Lilac = new WUIEngineColor(200, 162, 200, 255);
        private static readonly WUIEngineColor LilacLuster = new WUIEngineColor(174, 152, 170, 255);
        private static readonly WUIEngineColor LimeGreen = new WUIEngineColor(50, 205, 50, 255);
        private static readonly WUIEngineColor Limerick = new WUIEngineColor(157, 194, 9, 255);
        private static readonly WUIEngineColor LincolnGreen = new WUIEngineColor(25, 89, 5, 255);
        private static readonly WUIEngineColor Linen = new WUIEngineColor(250, 240, 230, 255);
        private static readonly WUIEngineColor LittleBoyBlue = new WUIEngineColor(108, 160, 220, 255);
        private static readonly WUIEngineColor LittleGirlPink = new WUIEngineColor(248, 185, 212, 255);
        private static readonly WUIEngineColor Liver = new WUIEngineColor(103, 76, 71, 255);
        private static readonly WUIEngineColor LiverDogs = new WUIEngineColor(184, 109, 41, 255);
        private static readonly WUIEngineColor LiverOrgan = new WUIEngineColor(108, 46, 31, 255);
        private static readonly WUIEngineColor LiverChestnut = new WUIEngineColor(152, 116, 86, 255);
        private static readonly WUIEngineColor Lotion = new WUIEngineColor(255, 254, 250, 255);
        private static readonly WUIEngineColor Lumber = new WUIEngineColor(255, 228, 205, 255);
        private static readonly WUIEngineColor Lust = new WUIEngineColor(230, 32, 32, 255);
        private static readonly WUIEngineColor MaastrichtBlue = new WUIEngineColor(0, 28, 61, 255);
        private static readonly WUIEngineColor MacaroniAndCheese = new WUIEngineColor(255, 189, 136, 255);
        private static readonly WUIEngineColor MadderLake = new WUIEngineColor(204, 51, 54, 255);
        private static readonly WUIEngineColor MagentaDye = new WUIEngineColor(202, 31, 123, 255);
        private static readonly WUIEngineColor MagentaPantone = new WUIEngineColor(208, 65, 126, 255);
        private static readonly WUIEngineColor MagentaProcess = new WUIEngineColor(255, 0, 144, 255);
        private static readonly WUIEngineColor MagentaHaze = new WUIEngineColor(159, 69, 118, 255);
        private static readonly WUIEngineColor MagentaPink = new WUIEngineColor(204, 51, 139, 255);
        private static readonly WUIEngineColor MagicMint = new WUIEngineColor(170, 240, 209, 255);
        private static readonly WUIEngineColor MagicPotion = new WUIEngineColor(255, 68, 102, 255);
        private static readonly WUIEngineColor Magnolia = new WUIEngineColor(248, 244, 255, 255);
        private static readonly WUIEngineColor Mahogany = new WUIEngineColor(192, 64, 0, 255);
        private static readonly WUIEngineColor MaizeCrayola = new WUIEngineColor(242, 198, 73, 255);
        private static readonly WUIEngineColor MajorelleBlue = new WUIEngineColor(96, 80, 220, 255);
        private static readonly WUIEngineColor Malachite = new WUIEngineColor(11, 218, 81, 255);
        private static readonly WUIEngineColor Manatee = new WUIEngineColor(151, 154, 170, 255);
        private static readonly WUIEngineColor Mandarin = new WUIEngineColor(243, 122, 72, 255);
        private static readonly WUIEngineColor MangoGreen = new WUIEngineColor(150, 255, 0, 255);
        private static readonly WUIEngineColor MangoTango = new WUIEngineColor(255, 130, 67, 255);
        private static readonly WUIEngineColor Mantis = new WUIEngineColor(116, 195, 101, 255);
        private static readonly WUIEngineColor MardiGras = new WUIEngineColor(136, 0, 133, 255);
        private static readonly WUIEngineColor Marigold = new WUIEngineColor(234, 162, 33, 255);
        private static readonly WUIEngineColor MaroonHTMLCSS = new WUIEngineColor(128, 0, 0, 255);
        private static readonly WUIEngineColor MaroonX11 = new WUIEngineColor(176, 48, 96, 255);
        private static readonly WUIEngineColor Mauve = new WUIEngineColor(224, 176, 255, 255);
        private static readonly WUIEngineColor MauveTaupe = new WUIEngineColor(145, 95, 109, 255);
        private static readonly WUIEngineColor Mauvelous = new WUIEngineColor(239, 152, 170, 255);
        private static readonly WUIEngineColor MaximumBlue = new WUIEngineColor(71, 171, 204, 255);
        private static readonly WUIEngineColor MaximumBlueGreen = new WUIEngineColor(48, 191, 191, 255);
        private static readonly WUIEngineColor MaximumBluePurple = new WUIEngineColor(172, 172, 230, 255);
        private static readonly WUIEngineColor MaximumGreen = new WUIEngineColor(94, 140, 49, 255);
        private static readonly WUIEngineColor MaximumGreenYellow = new WUIEngineColor(217, 230, 80, 255);
        private static readonly WUIEngineColor MaximumPurple = new WUIEngineColor(115, 51, 128, 255);
        private static readonly WUIEngineColor MaximumRed = new WUIEngineColor(217, 33, 33, 255);
        private static readonly WUIEngineColor MaximumRedPurple = new WUIEngineColor(166, 58, 121, 255);
        private static readonly WUIEngineColor MaximumYellow = new WUIEngineColor(250, 250, 55, 255);
        private static readonly WUIEngineColor MaximumYellowRed = new WUIEngineColor(242, 186, 73, 255);
        private static readonly WUIEngineColor MayGreen = new WUIEngineColor(76, 145, 65, 255);
        private static readonly WUIEngineColor MayaBlue = new WUIEngineColor(115, 194, 251, 255);
        private static readonly WUIEngineColor MeatBrown = new WUIEngineColor(229, 183, 59, 255);
        private static readonly WUIEngineColor MediumAquamarine = new WUIEngineColor(102, 221, 170, 255);
        private static readonly WUIEngineColor MediumBlue = new WUIEngineColor(0, 0, 205, 255);
        private static readonly WUIEngineColor MediumCandyAppleRed = new WUIEngineColor(226, 6, 44, 255);
        private static readonly WUIEngineColor MediumCarmine = new WUIEngineColor(175, 64, 53, 255);
        private static readonly WUIEngineColor MediumChampagne = new WUIEngineColor(243, 229, 171, 255);
        private static readonly WUIEngineColor MediumElectricBlue = new WUIEngineColor(3, 80, 150, 255);
        private static readonly WUIEngineColor MediumJungleGreen = new WUIEngineColor(28, 53, 45, 255);
        private static readonly WUIEngineColor MediumLavenderMagenta = new WUIEngineColor(221, 160, 221, 255);
        private static readonly WUIEngineColor MediumOrchid = new WUIEngineColor(186, 85, 211, 255);
        private static readonly WUIEngineColor MediumPersianBlue = new WUIEngineColor(0, 103, 165, 255);
        private static readonly WUIEngineColor MediumPurple = new WUIEngineColor(147, 112, 219, 255);
        private static readonly WUIEngineColor MediumRedViolet = new WUIEngineColor(187, 51, 133, 255);
        private static readonly WUIEngineColor MediumRuby = new WUIEngineColor(170, 64, 105, 255);
        private static readonly WUIEngineColor MediumSeaGreen = new WUIEngineColor(60, 179, 113, 255);
        private static readonly WUIEngineColor MediumSkyBlue = new WUIEngineColor(128, 218, 235, 255);
        private static readonly WUIEngineColor MediumSlateBlue = new WUIEngineColor(123, 104, 238, 255);
        private static readonly WUIEngineColor MediumSpringBud = new WUIEngineColor(201, 220, 135, 255);
        private static readonly WUIEngineColor MediumSpringGreen = new WUIEngineColor(0, 250, 154, 255);
        private static readonly WUIEngineColor MediumTurquoise = new WUIEngineColor(72, 209, 204, 255);
        private static readonly WUIEngineColor MediumVermilion = new WUIEngineColor(217, 96, 59, 255);
        private static readonly WUIEngineColor MediumVioletRed = new WUIEngineColor(199, 21, 133, 255);
        private static readonly WUIEngineColor MellowApricot = new WUIEngineColor(248, 184, 120, 255);
        private static readonly WUIEngineColor Melon = new WUIEngineColor(253, 188, 180, 255);
        private static readonly WUIEngineColor Menthol = new WUIEngineColor(193, 249, 162, 255);
        private static readonly WUIEngineColor MetallicBlue = new WUIEngineColor(50, 82, 123, 255);
        private static readonly WUIEngineColor MetallicBronze = new WUIEngineColor(169, 113, 66, 255);
        private static readonly WUIEngineColor MetallicBrown = new WUIEngineColor(172, 67, 19, 255);
        private static readonly WUIEngineColor MetallicGreen = new WUIEngineColor(41, 110, 1, 255);
        private static readonly WUIEngineColor MetallicOrange = new WUIEngineColor(218, 104, 15, 255);
        private static readonly WUIEngineColor MetallicPink = new WUIEngineColor(237, 166, 196, 255);
        private static readonly WUIEngineColor MetallicRed = new WUIEngineColor(166, 44, 43, 255);
        private static readonly WUIEngineColor MetallicSeaweed = new WUIEngineColor(10, 126, 140, 255);
        private static readonly WUIEngineColor MetallicSilver = new WUIEngineColor(168, 169, 173, 255);
        private static readonly WUIEngineColor MetallicSunburst = new WUIEngineColor(156, 124, 56, 255);
        private static readonly WUIEngineColor MetallicViolet = new WUIEngineColor(90, 10, 145, 255);
        private static readonly WUIEngineColor MetallicYellow = new WUIEngineColor(253, 204, 13, 255);
        private static readonly WUIEngineColor MexicanPink = new WUIEngineColor(228, 0, 124, 255);
        private static readonly WUIEngineColor MiddleBlue = new WUIEngineColor(126, 212, 230, 255);
        private static readonly WUIEngineColor MiddleBlueGreen = new WUIEngineColor(141, 217, 204, 255);
        private static readonly WUIEngineColor MiddleBluePurple = new WUIEngineColor(139, 114, 190, 255);
        private static readonly WUIEngineColor MiddleGrey = new WUIEngineColor(139, 134, 128, 255);
        private static readonly WUIEngineColor MiddleGreen = new WUIEngineColor(77, 140, 87, 255);
        private static readonly WUIEngineColor MiddleGreenYellow = new WUIEngineColor(172, 191, 96, 255);
        private static readonly WUIEngineColor MiddlePurple = new WUIEngineColor(217, 130, 181, 255);
        private static readonly WUIEngineColor MiddleRed = new WUIEngineColor(229, 144, 115, 255);
        private static readonly WUIEngineColor MiddleRedPurple = new WUIEngineColor(165, 83, 83, 255);
        private static readonly WUIEngineColor MiddleYellow = new WUIEngineColor(255, 235, 0, 255);
        private static readonly WUIEngineColor MiddleYellowRed = new WUIEngineColor(236, 177, 118, 255);
        private static readonly WUIEngineColor Midnight = new WUIEngineColor(112, 38, 112, 255);
        private static readonly WUIEngineColor MidnightBlue = new WUIEngineColor(25, 25, 112, 255);
        private static readonly WUIEngineColor MidnightBlue2 = new WUIEngineColor(0, 70, 140, 255);
        private static readonly WUIEngineColor MidnightGreenEagleGreen = new WUIEngineColor(0, 73, 83, 255);
        private static readonly WUIEngineColor MikadoYellow = new WUIEngineColor(255, 196, 12, 255);
        private static readonly WUIEngineColor Milk = new WUIEngineColor(253, 255, 245, 255);
        private static readonly WUIEngineColor MilkChocolate = new WUIEngineColor(132, 86, 60, 255);
        private static readonly WUIEngineColor MimiPink = new WUIEngineColor(255, 218, 233, 255);
        private static readonly WUIEngineColor Mindaro = new WUIEngineColor(227, 249, 136, 255);
        private static readonly WUIEngineColor Ming = new WUIEngineColor(54, 116, 125, 255);
        private static readonly WUIEngineColor MinionYellow = new WUIEngineColor(245, 220, 80, 255);
        private static readonly WUIEngineColor Mint = new WUIEngineColor(62, 180, 137, 255);
        private static readonly WUIEngineColor MintCream = new WUIEngineColor(245, 255, 250, 255);
        private static readonly WUIEngineColor MintGreen = new WUIEngineColor(152, 255, 152, 255);
        private static readonly WUIEngineColor MistyMoss = new WUIEngineColor(187, 180, 119, 255);
        private static readonly WUIEngineColor MistyRose = new WUIEngineColor(255, 228, 225, 255);
        private static readonly WUIEngineColor Moonstone = new WUIEngineColor(58, 168, 193, 255);
        private static readonly WUIEngineColor MoonstoneBlue = new WUIEngineColor(115, 169, 194, 255);
        private static readonly WUIEngineColor MordantRed19 = new WUIEngineColor(174, 12, 0, 255);
        private static readonly WUIEngineColor MorningBlue = new WUIEngineColor(141, 163, 153, 255);
        private static readonly WUIEngineColor MossGreen = new WUIEngineColor(138, 154, 91, 255);
        private static readonly WUIEngineColor MountainMeadow = new WUIEngineColor(48, 186, 143, 255);
        private static readonly WUIEngineColor MountbattenPink = new WUIEngineColor(153, 122, 141, 255);
        private static readonly WUIEngineColor MSUGreen = new WUIEngineColor(24, 69, 59, 255);
        private static readonly WUIEngineColor Mud = new WUIEngineColor(111, 83, 61, 255);
        private static readonly WUIEngineColor MughalGreen = new WUIEngineColor(48, 96, 48, 255);
        private static readonly WUIEngineColor Mulberry = new WUIEngineColor(197, 75, 140, 255);
        private static readonly WUIEngineColor MulberryCrayola = new WUIEngineColor(200, 80, 155, 255);
        private static readonly WUIEngineColor Mustard = new WUIEngineColor(255, 219, 88, 255);
        private static readonly WUIEngineColor MustardBrown = new WUIEngineColor(205, 122, 0, 255);
        private static readonly WUIEngineColor MustardGreen = new WUIEngineColor(110, 110, 48, 255);
        private static readonly WUIEngineColor MustardYellow = new WUIEngineColor(255, 173, 1, 255);
        private static readonly WUIEngineColor MyrtleGreen = new WUIEngineColor(49, 120, 115, 255);
        private static readonly WUIEngineColor Mystic = new WUIEngineColor(214, 82, 130, 255);
        private static readonly WUIEngineColor MysticMaroon = new WUIEngineColor(173, 67, 121, 255);
        private static readonly WUIEngineColor MysticRed = new WUIEngineColor(255, 34, 0, 255);
        private static readonly WUIEngineColor NadeshikoPink = new WUIEngineColor(246, 173, 198, 255);
        private static readonly WUIEngineColor NapierGreen = new WUIEngineColor(42, 128, 0, 255);
        private static readonly WUIEngineColor NaplesYellow = new WUIEngineColor(250, 218, 94, 255);
        private static readonly WUIEngineColor NavajoWhite = new WUIEngineColor(255, 222, 173, 255);
        private static readonly WUIEngineColor Navy = new WUIEngineColor(0, 0, 128, 255);
        private static readonly WUIEngineColor NeonBlue = new WUIEngineColor(27, 3, 163, 255);
        private static readonly WUIEngineColor NeonBrown = new WUIEngineColor(195, 115, 42, 255);
        private static readonly WUIEngineColor NeonCarrot = new WUIEngineColor(255, 163, 67, 255);
        private static readonly WUIEngineColor NeonCyan = new WUIEngineColor(0, 254, 252, 255);
        private static readonly WUIEngineColor NeonFuchsia = new WUIEngineColor(254, 65, 100, 255);
        private static readonly WUIEngineColor NeonGold = new WUIEngineColor(207, 170, 1, 255);
        private static readonly WUIEngineColor NeonGreen = new WUIEngineColor(57, 255, 20, 255);
        private static readonly WUIEngineColor NeonPink = new WUIEngineColor(254, 52, 126, 255);
        private static readonly WUIEngineColor NeonRed = new WUIEngineColor(255, 24, 24, 255);
        private static readonly WUIEngineColor NeonScarlet = new WUIEngineColor(255, 38, 3, 255);
        private static readonly WUIEngineColor NeonTangerine = new WUIEngineColor(246, 137, 10, 255);
        private static readonly WUIEngineColor NewCar = new WUIEngineColor(33, 79, 198, 255);
        private static readonly WUIEngineColor NewYorkPink = new WUIEngineColor(215, 131, 127, 255);
        private static readonly WUIEngineColor Nickel = new WUIEngineColor(114, 116, 114, 255);
        private static readonly WUIEngineColor NonPhotoBlue = new WUIEngineColor(164, 221, 237, 255);
        private static readonly WUIEngineColor NorthTexasGreen = new WUIEngineColor(5, 144, 51, 255);
        private static readonly WUIEngineColor Nyanza = new WUIEngineColor(233, 255, 219, 255);
        private static readonly WUIEngineColor OceanBlue = new WUIEngineColor(79, 66, 181, 255);
        private static readonly WUIEngineColor OceanBoatBlue = new WUIEngineColor(0, 119, 190, 255);
        private static readonly WUIEngineColor OceanGreen = new WUIEngineColor(72, 191, 145, 255);
        private static readonly WUIEngineColor Ochre = new WUIEngineColor(204, 119, 34, 255);
        private static readonly WUIEngineColor OgreOdor = new WUIEngineColor(253, 82, 64, 255);
        private static readonly WUIEngineColor OldBurgundy = new WUIEngineColor(67, 48, 46, 255);
        private static readonly WUIEngineColor OldGold = new WUIEngineColor(207, 181, 59, 255);
        private static readonly WUIEngineColor OldLace = new WUIEngineColor(253, 245, 230, 255);
        private static readonly WUIEngineColor OldLavender = new WUIEngineColor(121, 104, 120, 255);
        private static readonly WUIEngineColor OldMauve = new WUIEngineColor(103, 49, 71, 255);
        private static readonly WUIEngineColor OldMossGreen = new WUIEngineColor(134, 126, 54, 255);
        private static readonly WUIEngineColor OldRose = new WUIEngineColor(192, 128, 129, 255);
        private static readonly WUIEngineColor OliveDrab3 = new WUIEngineColor(107, 142, 35, 255);
        private static readonly WUIEngineColor OliveDrab7 = new WUIEngineColor(60, 52, 31, 255);
        private static readonly WUIEngineColor Olivine = new WUIEngineColor(154, 185, 115, 255);
        private static readonly WUIEngineColor Onyx = new WUIEngineColor(53, 56, 57, 255);
        private static readonly WUIEngineColor Opal = new WUIEngineColor(168, 195, 188, 255);
        private static readonly WUIEngineColor OperaMauve = new WUIEngineColor(183, 132, 167, 255);
        private static readonly WUIEngineColor OrangeColorWheel = new WUIEngineColor(255, 127, 0, 255);
        private static readonly WUIEngineColor OrangeCrayola = new WUIEngineColor(255, 117, 56, 255);
        private static readonly WUIEngineColor OrangePantone = new WUIEngineColor(255, 88, 0, 255);
        private static readonly WUIEngineColor OrangeRYB = new WUIEngineColor(251, 153, 2, 255);
        private static readonly WUIEngineColor OrangeWeb = new WUIEngineColor(255, 165, 0, 255);
        private static readonly WUIEngineColor OrangePeel = new WUIEngineColor(255, 159, 0, 255);
        private static readonly WUIEngineColor OrangeRed = new WUIEngineColor(255, 69, 0, 255);
        private static readonly WUIEngineColor OrangeSoda = new WUIEngineColor(250, 91, 61, 255);
        private static readonly WUIEngineColor OrangeYellow = new WUIEngineColor(248, 213, 104, 255);
        private static readonly WUIEngineColor Orchid = new WUIEngineColor(218, 112, 214, 255);
        private static readonly WUIEngineColor OrchidPink = new WUIEngineColor(242, 189, 205, 255);
        private static readonly WUIEngineColor OriolesOrange = new WUIEngineColor(251, 79, 20, 255);
        private static readonly WUIEngineColor OuterSpace = new WUIEngineColor(65, 74, 76, 255);
        private static readonly WUIEngineColor OutrageousOrange = new WUIEngineColor(255, 110, 74, 255);
        private static readonly WUIEngineColor OxfordBlue = new WUIEngineColor(0, 33, 71, 255);
        private static readonly WUIEngineColor Oxley = new WUIEngineColor(109, 154, 121, 255);
        private static readonly WUIEngineColor PacificBlue = new WUIEngineColor(28, 169, 201, 255);
        private static readonly WUIEngineColor PakistanGreen = new WUIEngineColor(0, 102, 0, 255);
        private static readonly WUIEngineColor PalatinateBlue = new WUIEngineColor(39, 59, 226, 255);
        private static readonly WUIEngineColor PalatinatePurple = new WUIEngineColor(104, 40, 96, 255);
        private static readonly WUIEngineColor PaleBlue = new WUIEngineColor(175, 238, 238, 255);
        private static readonly WUIEngineColor PaleBrown = new WUIEngineColor(152, 118, 84, 255);
        private static readonly WUIEngineColor PaleCerulean = new WUIEngineColor(155, 196, 226, 255);
        private static readonly WUIEngineColor PaleChestnut = new WUIEngineColor(221, 173, 175, 255);
        private static readonly WUIEngineColor PaleCornflowerBlue = new WUIEngineColor(171, 205, 239, 255);
        private static readonly WUIEngineColor PaleCyan = new WUIEngineColor(135, 211, 248, 255);
        private static readonly WUIEngineColor PaleGoldenrod = new WUIEngineColor(238, 232, 170, 255);
        private static readonly WUIEngineColor PaleGreen = new WUIEngineColor(152, 251, 152, 255);
        private static readonly WUIEngineColor PaleLavender = new WUIEngineColor(220, 208, 255, 255);
        private static readonly WUIEngineColor PaleMagenta = new WUIEngineColor(249, 132, 229, 255);
        private static readonly WUIEngineColor PaleMagentaPink = new WUIEngineColor(255, 153, 204, 255);
        private static readonly WUIEngineColor PalePink = new WUIEngineColor(250, 218, 221, 255);
        private static readonly WUIEngineColor PaleRedViolet = new WUIEngineColor(219, 112, 147, 255);
        private static readonly WUIEngineColor PaleRobinEggBlue = new WUIEngineColor(150, 222, 209, 255);
        private static readonly WUIEngineColor PaleSilver = new WUIEngineColor(201, 192, 187, 255);
        private static readonly WUIEngineColor PaleSpringBud = new WUIEngineColor(236, 235, 189, 255);
        private static readonly WUIEngineColor PaleTaupe = new WUIEngineColor(188, 152, 126, 255);
        private static readonly WUIEngineColor PaleViolet = new WUIEngineColor(204, 153, 255, 255);
        private static readonly WUIEngineColor PalmLeaf = new WUIEngineColor(111, 153, 64, 255);
        private static readonly WUIEngineColor PansyPurple = new WUIEngineColor(120, 24, 74, 255);
        private static readonly WUIEngineColor PaoloVeroneseGreen = new WUIEngineColor(0, 155, 125, 255);
        private static readonly WUIEngineColor PapayaWhip = new WUIEngineColor(255, 239, 213, 255);
        private static readonly WUIEngineColor ParadisePink = new WUIEngineColor(230, 62, 98, 255);
        private static readonly WUIEngineColor ParrotPink = new WUIEngineColor(217, 152, 160, 255);
        private static readonly WUIEngineColor PastelBlue = new WUIEngineColor(174, 198, 207, 255);
        private static readonly WUIEngineColor PastelBrown = new WUIEngineColor(130, 105, 83, 255);
        private static readonly WUIEngineColor PastelGray = new WUIEngineColor(207, 207, 196, 255);
        private static readonly WUIEngineColor PastelGreen = new WUIEngineColor(119, 221, 119, 255);
        private static readonly WUIEngineColor PastelMagenta = new WUIEngineColor(244, 154, 194, 255);
        private static readonly WUIEngineColor PastelOrange = new WUIEngineColor(255, 179, 71, 255);
        private static readonly WUIEngineColor PastelPink = new WUIEngineColor(222, 165, 164, 255);
        private static readonly WUIEngineColor PastelPurple = new WUIEngineColor(179, 158, 181, 255);
        private static readonly WUIEngineColor PastelRed = new WUIEngineColor(255, 105, 97, 255);
        private static readonly WUIEngineColor PastelViolet = new WUIEngineColor(203, 153, 201, 255);
        private static readonly WUIEngineColor PastelYellow = new WUIEngineColor(253, 253, 150, 255);
        private static readonly WUIEngineColor Patriarch = new WUIEngineColor(128, 0, 128, 255);
        private static readonly WUIEngineColor Peach = new WUIEngineColor(255, 229, 180, 255);
        private static readonly WUIEngineColor PeachOrange = new WUIEngineColor(255, 204, 153, 255);
        private static readonly WUIEngineColor PeachPuff = new WUIEngineColor(255, 218, 185, 255);
        private static readonly WUIEngineColor PeachYellow = new WUIEngineColor(250, 223, 173, 255);
        private static readonly WUIEngineColor Pear = new WUIEngineColor(209, 226, 49, 255);
        private static readonly WUIEngineColor Pearl = new WUIEngineColor(234, 224, 200, 255);
        private static readonly WUIEngineColor PearlAqua = new WUIEngineColor(136, 216, 192, 255);
        private static readonly WUIEngineColor PearlyPurple = new WUIEngineColor(183, 104, 162, 255);
        private static readonly WUIEngineColor Peridot = new WUIEngineColor(230, 226, 0, 255);
        private static readonly WUIEngineColor PeriwinkleCrayola = new WUIEngineColor(195, 205, 230, 255);
        private static readonly WUIEngineColor PermanentGeraniumLake = new WUIEngineColor(225, 44, 44, 255);
        private static readonly WUIEngineColor PersianBlue = new WUIEngineColor(28, 57, 187, 255);
        private static readonly WUIEngineColor PersianGreen = new WUIEngineColor(0, 166, 147, 255);
        private static readonly WUIEngineColor PersianIndigo = new WUIEngineColor(50, 18, 122, 255);
        private static readonly WUIEngineColor PersianOrange = new WUIEngineColor(217, 144, 88, 255);
        private static readonly WUIEngineColor PersianPink = new WUIEngineColor(247, 127, 190, 255);
        private static readonly WUIEngineColor PersianPlum = new WUIEngineColor(112, 28, 28, 255);
        private static readonly WUIEngineColor PersianRed = new WUIEngineColor(204, 51, 51, 255);
        private static readonly WUIEngineColor PersianRose = new WUIEngineColor(254, 40, 162, 255);
        private static readonly WUIEngineColor Persimmon = new WUIEngineColor(236, 88, 0, 255);
        private static readonly WUIEngineColor Peru = new WUIEngineColor(205, 133, 63, 255);
        private static readonly WUIEngineColor PewterBlue = new WUIEngineColor(139, 168, 183, 255);
        private static readonly WUIEngineColor PhilippineBlue = new WUIEngineColor(0, 56, 167, 255);
        private static readonly WUIEngineColor PhilippineBrown = new WUIEngineColor(93, 25, 22, 255);
        private static readonly WUIEngineColor PhilippineGold = new WUIEngineColor(177, 115, 4, 255);
        private static readonly WUIEngineColor PhilippineGoldenYellow = new WUIEngineColor(253, 223, 22, 255);
        private static readonly WUIEngineColor PhilippineGray = new WUIEngineColor(140, 140, 140, 255);
        private static readonly WUIEngineColor PhilippineGreen = new WUIEngineColor(0, 133, 67, 255);
        private static readonly WUIEngineColor PhilippineOrange = new WUIEngineColor(255, 115, 0, 255);
        private static readonly WUIEngineColor PhilippinePink = new WUIEngineColor(255, 26, 142, 255);
        private static readonly WUIEngineColor PhilippineRed = new WUIEngineColor(206, 17, 39, 255);
        private static readonly WUIEngineColor PhilippineSilver = new WUIEngineColor(179, 179, 179, 255);
        private static readonly WUIEngineColor PhilippineViolet = new WUIEngineColor(129, 0, 127, 255);
        private static readonly WUIEngineColor PhilippineYellow = new WUIEngineColor(254, 203, 0, 255);
        private static readonly WUIEngineColor Phlox = new WUIEngineColor(223, 0, 255, 255);
        private static readonly WUIEngineColor PhthaloBlue = new WUIEngineColor(0, 15, 137, 255);
        private static readonly WUIEngineColor PhthaloGreen = new WUIEngineColor(18, 53, 36, 255);
        private static readonly WUIEngineColor PictonBlue = new WUIEngineColor(69, 177, 232, 255);
        private static readonly WUIEngineColor PictorialCarmine = new WUIEngineColor(195, 11, 78, 255);
        private static readonly WUIEngineColor PiggyPink = new WUIEngineColor(253, 221, 230, 255);
        private static readonly WUIEngineColor PineGreen = new WUIEngineColor(1, 121, 111, 255);
        private static readonly WUIEngineColor PineTree = new WUIEngineColor(42, 47, 35, 255);
        private static readonly WUIEngineColor Pineapple = new WUIEngineColor(86, 60, 13, 255);
        private static readonly WUIEngineColor Pink = new WUIEngineColor(255, 192, 203, 255);
        private static readonly WUIEngineColor PinkPantone = new WUIEngineColor(215, 72, 148, 255);
        private static readonly WUIEngineColor PinkFlamingo = new WUIEngineColor(252, 116, 253, 255);
        private static readonly WUIEngineColor PinkLace = new WUIEngineColor(255, 221, 244, 255);
        private static readonly WUIEngineColor PinkLavender = new WUIEngineColor(216, 178, 209, 255);
        private static readonly WUIEngineColor PinkPearl = new WUIEngineColor(231, 172, 207, 255);
        private static readonly WUIEngineColor PinkRaspberry = new WUIEngineColor(152, 0, 54, 255);
        private static readonly WUIEngineColor PinkSherbet = new WUIEngineColor(247, 143, 167, 255);
        private static readonly WUIEngineColor Pistachio = new WUIEngineColor(147, 197, 114, 255);
        private static readonly WUIEngineColor PixiePowder = new WUIEngineColor(57, 18, 133, 255);
        private static readonly WUIEngineColor Platinum = new WUIEngineColor(229, 228, 226, 255);
        private static readonly WUIEngineColor Plum = new WUIEngineColor(142, 69, 133, 255);
        private static readonly WUIEngineColor PlumpPurple = new WUIEngineColor(89, 70, 178, 255);
        private static readonly WUIEngineColor PoliceBlue = new WUIEngineColor(55, 79, 107, 255);
        private static readonly WUIEngineColor PolishedPine = new WUIEngineColor(93, 164, 147, 255);
        private static readonly WUIEngineColor Popstar = new WUIEngineColor(190, 79, 98, 255);
        private static readonly WUIEngineColor PortlandOrange = new WUIEngineColor(255, 90, 54, 255);
        private static readonly WUIEngineColor PowderBlue = new WUIEngineColor(176, 224, 230, 255);
        private static readonly WUIEngineColor PrincessPerfume = new WUIEngineColor(255, 133, 207, 255);
        private static readonly WUIEngineColor PrincetonOrange = new WUIEngineColor(245, 128, 37, 255);
        private static readonly WUIEngineColor PrussianBlue = new WUIEngineColor(0, 49, 83, 255);
        private static readonly WUIEngineColor Puce = new WUIEngineColor(204, 136, 153, 255);
        private static readonly WUIEngineColor PuceRed = new WUIEngineColor(114, 47, 55, 255);
        private static readonly WUIEngineColor PullmanBrownUPSBrown = new WUIEngineColor(100, 65, 23, 255);
        private static readonly WUIEngineColor PullmanGreen = new WUIEngineColor(59, 51, 28, 255);
        private static readonly WUIEngineColor Pumpkin = new WUIEngineColor(255, 117, 24, 255);
        private static readonly WUIEngineColor PurpleMunsell = new WUIEngineColor(159, 0, 197, 255);
        private static readonly WUIEngineColor PurpleX11 = new WUIEngineColor(160, 32, 240, 255);
        private static readonly WUIEngineColor PurpleHeart = new WUIEngineColor(105, 53, 156, 255);
        private static readonly WUIEngineColor PurpleMountainMajesty = new WUIEngineColor(150, 120, 182, 255);
        private static readonly WUIEngineColor PurpleNavy = new WUIEngineColor(78, 81, 128, 255);
        private static readonly WUIEngineColor PurplePizzazz = new WUIEngineColor(254, 78, 218, 255);
        private static readonly WUIEngineColor PurplePlum = new WUIEngineColor(156, 81, 182, 255);
        private static readonly WUIEngineColor PurpleTaupe = new WUIEngineColor(80, 64, 77, 255);
        private static readonly WUIEngineColor Purpureus = new WUIEngineColor(154, 78, 174, 255);
        private static readonly WUIEngineColor Quartz = new WUIEngineColor(81, 72, 79, 255);
        private static readonly WUIEngineColor QueenBlue = new WUIEngineColor(67, 107, 149, 255);
        private static readonly WUIEngineColor QueenPink = new WUIEngineColor(232, 204, 215, 255);
        private static readonly WUIEngineColor QuickSilver = new WUIEngineColor(166, 166, 166, 255);
        private static readonly WUIEngineColor QuinacridoneMagenta = new WUIEngineColor(142, 58, 89, 255);
        private static readonly WUIEngineColor Quincy = new WUIEngineColor(106, 84, 69, 255);
        private static readonly WUIEngineColor RadicalRed = new WUIEngineColor(255, 53, 94, 255);
        private static readonly WUIEngineColor RaisinBlack = new WUIEngineColor(36, 33, 36, 255);
        private static readonly WUIEngineColor Rajah = new WUIEngineColor(251, 171, 96, 255);
        private static readonly WUIEngineColor Raspberry = new WUIEngineColor(227, 11, 92, 255);
        private static readonly WUIEngineColor RaspberryPink = new WUIEngineColor(226, 80, 152, 255);
        private static readonly WUIEngineColor RawSienna = new WUIEngineColor(214, 138, 89, 255);
        private static readonly WUIEngineColor RawUmber = new WUIEngineColor(130, 102, 68, 255);
        private static readonly WUIEngineColor RazzleDazzleRose = new WUIEngineColor(255, 51, 204, 255);
        private static readonly WUIEngineColor Razzmatazz = new WUIEngineColor(227, 37, 107, 255);
        private static readonly WUIEngineColor RazzmicBerry = new WUIEngineColor(141, 78, 133, 255);
        private static readonly WUIEngineColor RebeccaPurple = new WUIEngineColor(102, 52, 153, 255);
        private static readonly WUIEngineColor Red = new WUIEngineColor(255, 0, 0, 255);
        private static readonly WUIEngineColor RedCrayola = new WUIEngineColor(238, 32, 77, 255);
        private static readonly WUIEngineColor RedMunsell = new WUIEngineColor(242, 0, 60, 255);
        private static readonly WUIEngineColor RedNCS = new WUIEngineColor(196, 2, 51, 255);
        private static readonly WUIEngineColor RedPigment = new WUIEngineColor(237, 28, 36, 255);
        private static readonly WUIEngineColor RedRYB = new WUIEngineColor(254, 39, 18, 255);
        private static readonly WUIEngineColor RedDevil = new WUIEngineColor(134, 1, 17, 255);
        private static readonly WUIEngineColor RedOrange = new WUIEngineColor(255, 83, 73, 255);
        private static readonly WUIEngineColor RedPurple = new WUIEngineColor(228, 0, 120, 255);
        private static readonly WUIEngineColor RedSalsa = new WUIEngineColor(253, 58, 74, 255);
        private static readonly WUIEngineColor Redwood = new WUIEngineColor(164, 90, 82, 255);
        private static readonly WUIEngineColor Regalia = new WUIEngineColor(82, 45, 128, 255);
        private static readonly WUIEngineColor ResolutionBlue = new WUIEngineColor(0, 35, 135, 255);
        private static readonly WUIEngineColor Rhythm = new WUIEngineColor(119, 118, 150, 255);
        private static readonly WUIEngineColor RichBlack = new WUIEngineColor(0, 64, 64, 255);
        private static readonly WUIEngineColor RichBlackFOGRA29 = new WUIEngineColor(1, 11, 19, 255);
        private static readonly WUIEngineColor RichBlackFOGRA39 = new WUIEngineColor(1, 2, 3, 255);
        private static readonly WUIEngineColor RichBrilliantLavender = new WUIEngineColor(241, 167, 254, 255);
        private static readonly WUIEngineColor RichElectricBlue = new WUIEngineColor(8, 146, 208, 255);
        private static readonly WUIEngineColor RichLavender = new WUIEngineColor(167, 107, 207, 255);
        private static readonly WUIEngineColor RichLilac = new WUIEngineColor(182, 102, 210, 255);
        private static readonly WUIEngineColor RifleGreen = new WUIEngineColor(68, 76, 56, 255);
        private static readonly WUIEngineColor RobinEggBlue = new WUIEngineColor(0, 204, 204, 255);
        private static readonly WUIEngineColor RocketMetallic = new WUIEngineColor(138, 127, 128, 255);
        private static readonly WUIEngineColor RomanSilver = new WUIEngineColor(131, 137, 150, 255);
        private static readonly WUIEngineColor RootBeer = new WUIEngineColor(41, 14, 5, 255);
        private static readonly WUIEngineColor RoseBonbon = new WUIEngineColor(249, 66, 158, 255);
        private static readonly WUIEngineColor RoseDust = new WUIEngineColor(158, 94, 111, 255);
        private static readonly WUIEngineColor RoseEbony = new WUIEngineColor(103, 72, 70, 255);
        private static readonly WUIEngineColor RoseGarnet = new WUIEngineColor(150, 1, 69, 255);
        private static readonly WUIEngineColor RoseGold = new WUIEngineColor(183, 110, 121, 255);
        private static readonly WUIEngineColor RosePink = new WUIEngineColor(255, 102, 204, 255);
        private static readonly WUIEngineColor RoseQuartz = new WUIEngineColor(170, 152, 169, 255);
        private static readonly WUIEngineColor RoseQuartzPink = new WUIEngineColor(189, 85, 156, 255);
        private static readonly WUIEngineColor RoseRed = new WUIEngineColor(194, 30, 86, 255);
        private static readonly WUIEngineColor RoseTaupe = new WUIEngineColor(144, 93, 93, 255);
        private static readonly WUIEngineColor RoseVale = new WUIEngineColor(171, 78, 82, 255);
        private static readonly WUIEngineColor Rosewood = new WUIEngineColor(101, 0, 11, 255);
        private static readonly WUIEngineColor RossoCorsa = new WUIEngineColor(212, 0, 0, 255);
        private static readonly WUIEngineColor RosyBrown = new WUIEngineColor(188, 143, 143, 255);
        private static readonly WUIEngineColor RoyalAzure = new WUIEngineColor(0, 56, 168, 255);
        private static readonly WUIEngineColor RoyalBlue = new WUIEngineColor(0, 35, 102, 255);
        private static readonly WUIEngineColor RoyalBlue2 = new WUIEngineColor(65, 105, 225, 255);
        private static readonly WUIEngineColor RoyalBrown = new WUIEngineColor(82, 59, 53, 255);
        private static readonly WUIEngineColor RoyalFuchsia = new WUIEngineColor(202, 44, 146, 255);
        private static readonly WUIEngineColor RoyalGreen = new WUIEngineColor(19, 98, 7, 255);
        private static readonly WUIEngineColor RoyalOrange = new WUIEngineColor(249, 146, 69, 255);
        private static readonly WUIEngineColor RoyalPink = new WUIEngineColor(231, 56, 149, 255);
        private static readonly WUIEngineColor RoyalRed = new WUIEngineColor(155, 28, 49, 255);
        private static readonly WUIEngineColor RoyalRed2 = new WUIEngineColor(208, 0, 96, 255);
        private static readonly WUIEngineColor RoyalPurple = new WUIEngineColor(120, 81, 169, 255);
        private static readonly WUIEngineColor Ruber = new WUIEngineColor(206, 70, 118, 255);
        private static readonly WUIEngineColor RubineRed = new WUIEngineColor(209, 0, 86, 255);
        private static readonly WUIEngineColor Ruby = new WUIEngineColor(224, 17, 95, 255);
        private static readonly WUIEngineColor RubyRed = new WUIEngineColor(155, 17, 30, 255);
        private static readonly WUIEngineColor Ruddy = new WUIEngineColor(255, 0, 40, 255);
        private static readonly WUIEngineColor RuddyBrown = new WUIEngineColor(187, 101, 40, 255);
        private static readonly WUIEngineColor RuddyPink = new WUIEngineColor(225, 142, 150, 255);
        private static readonly WUIEngineColor Rufous = new WUIEngineColor(168, 28, 7, 255);
        private static readonly WUIEngineColor Russet = new WUIEngineColor(128, 70, 27, 255);
        private static readonly WUIEngineColor RussianGreen = new WUIEngineColor(103, 146, 103, 255);
        private static readonly WUIEngineColor RussianViolet = new WUIEngineColor(50, 23, 77, 255);
        private static readonly WUIEngineColor Rust = new WUIEngineColor(183, 65, 14, 255);
        private static readonly WUIEngineColor RustyRed = new WUIEngineColor(218, 44, 67, 255);
        private static readonly WUIEngineColor SacramentoStateGreen = new WUIEngineColor(4, 57, 39, 255);
        private static readonly WUIEngineColor SaddleBrown = new WUIEngineColor(139, 69, 19, 255);
        private static readonly WUIEngineColor SafetyOrange = new WUIEngineColor(255, 120, 0, 255);
        private static readonly WUIEngineColor SafetyOrangeBlazeOrange = new WUIEngineColor(255, 103, 0, 255);
        private static readonly WUIEngineColor SafetyYellow = new WUIEngineColor(238, 210, 2, 255);
        private static readonly WUIEngineColor Saffron = new WUIEngineColor(244, 196, 48, 255);
        private static readonly WUIEngineColor Sage = new WUIEngineColor(188, 184, 138, 255);
        private static readonly WUIEngineColor StPatricksBlue = new WUIEngineColor(35, 41, 122, 255);
        private static readonly WUIEngineColor SalemColor = new WUIEngineColor(23, 123, 77, 255);
        private static readonly WUIEngineColor Salmon = new WUIEngineColor(250, 128, 114, 255);
        private static readonly WUIEngineColor SalmonPink = new WUIEngineColor(255, 145, 164, 255);
        private static readonly WUIEngineColor Sandstorm = new WUIEngineColor(236, 213, 64, 255);
        private static readonly WUIEngineColor SandyBrown = new WUIEngineColor(244, 164, 96, 255);
        private static readonly WUIEngineColor SandyTan = new WUIEngineColor(253, 217, 181, 255);
        private static readonly WUIEngineColor Sangria = new WUIEngineColor(146, 0, 10, 255);
        private static readonly WUIEngineColor SapGreen = new WUIEngineColor(80, 125, 42, 255);
        private static readonly WUIEngineColor Sapphire = new WUIEngineColor(15, 82, 186, 255);
        private static readonly WUIEngineColor SasquatchSocks = new WUIEngineColor(255, 70, 129, 255);
        private static readonly WUIEngineColor SatinSheenGold = new WUIEngineColor(203, 161, 53, 255);
        private static readonly WUIEngineColor Scarlet = new WUIEngineColor(255, 36, 0, 255);
        private static readonly WUIEngineColor Scarlet2 = new WUIEngineColor(253, 14, 53, 255);
        private static readonly WUIEngineColor SchoolBusYellow = new WUIEngineColor(255, 216, 0, 255);
        private static readonly WUIEngineColor ScreaminGreen = new WUIEngineColor(102, 255, 102, 255);
        private static readonly WUIEngineColor SeaBlue = new WUIEngineColor(0, 105, 148, 255);
        private static readonly WUIEngineColor SeaFoamGreen = new WUIEngineColor(195, 226, 191, 255);
        private static readonly WUIEngineColor SeaGreen = new WUIEngineColor(46, 139, 87, 255);
        private static readonly WUIEngineColor SeaGreenCrayola = new WUIEngineColor(1, 255, 205, 255);
        private static readonly WUIEngineColor SeaSerpent = new WUIEngineColor(75, 199, 207, 255);
        private static readonly WUIEngineColor SealBrown = new WUIEngineColor(50, 20, 20, 255);
        private static readonly WUIEngineColor Seashell = new WUIEngineColor(255, 245, 238, 255);
        private static readonly WUIEngineColor SelectiveYellow = new WUIEngineColor(255, 186, 0, 255);
        private static readonly WUIEngineColor Sepia = new WUIEngineColor(112, 66, 20, 255);
        private static readonly WUIEngineColor Shadow = new WUIEngineColor(138, 121, 93, 255);
        private static readonly WUIEngineColor ShadowBlue = new WUIEngineColor(119, 139, 165, 255);
        private static readonly WUIEngineColor Shampoo = new WUIEngineColor(255, 207, 241, 255);
        private static readonly WUIEngineColor ShamrockGreen = new WUIEngineColor(0, 158, 96, 255);
        private static readonly WUIEngineColor SheenGreen = new WUIEngineColor(143, 212, 0, 255);
        private static readonly WUIEngineColor ShimmeringBlush = new WUIEngineColor(217, 134, 149, 255);
        private static readonly WUIEngineColor ShinyShamrock = new WUIEngineColor(95, 167, 120, 255);
        private static readonly WUIEngineColor ShockingPink = new WUIEngineColor(252, 15, 192, 255);
        private static readonly WUIEngineColor ShockingPinkCrayola = new WUIEngineColor(255, 111, 255, 255);
        private static readonly WUIEngineColor Silver = new WUIEngineColor(192, 192, 192, 255);
        private static readonly WUIEngineColor SilverMetallic = new WUIEngineColor(170, 169, 173, 255);
        private static readonly WUIEngineColor SilverChalice = new WUIEngineColor(172, 172, 172, 255);
        private static readonly WUIEngineColor SilverFoil = new WUIEngineColor(175, 177, 174, 255);
        private static readonly WUIEngineColor SilverLakeBlue = new WUIEngineColor(93, 137, 186, 255);
        private static readonly WUIEngineColor SilverPink = new WUIEngineColor(196, 174, 173, 255);
        private static readonly WUIEngineColor SilverSand = new WUIEngineColor(191, 193, 194, 255);
        private static readonly WUIEngineColor Sinopia = new WUIEngineColor(203, 65, 11, 255);
        private static readonly WUIEngineColor SizzlingRed = new WUIEngineColor(255, 56, 85, 255);
        private static readonly WUIEngineColor SizzlingSunrise = new WUIEngineColor(255, 219, 0, 255);
        private static readonly WUIEngineColor Skobeloff = new WUIEngineColor(0, 116, 116, 255);
        private static readonly WUIEngineColor SkyBlue = new WUIEngineColor(135, 206, 235, 255);
        private static readonly WUIEngineColor SkyBlueCrayola = new WUIEngineColor(118, 215, 234, 255);
        private static readonly WUIEngineColor SkyMagenta = new WUIEngineColor(207, 113, 175, 255);
        private static readonly WUIEngineColor SlateBlue = new WUIEngineColor(106, 90, 205, 255);
        private static readonly WUIEngineColor SlateGray = new WUIEngineColor(112, 128, 144, 255);
        private static readonly WUIEngineColor SlimyGreen = new WUIEngineColor(41, 150, 23, 255);
        private static readonly WUIEngineColor SmashedPumpkin = new WUIEngineColor(255, 109, 58, 255);
        private static readonly WUIEngineColor Smitten = new WUIEngineColor(200, 65, 134, 255);
        private static readonly WUIEngineColor Smoke = new WUIEngineColor(115, 130, 118, 255);
        private static readonly WUIEngineColor SmokeyTopaz = new WUIEngineColor(131, 42, 34, 255);
        private static readonly WUIEngineColor SmokyBlack = new WUIEngineColor(16, 12, 8, 255);
        private static readonly WUIEngineColor SmokyTopaz = new WUIEngineColor(147, 61, 65, 255);
        private static readonly WUIEngineColor Snow = new WUIEngineColor(255, 250, 250, 255);
        private static readonly WUIEngineColor Soap = new WUIEngineColor(206, 200, 239, 255);
        private static readonly WUIEngineColor SoldierGreen = new WUIEngineColor(84, 90, 44, 255);
        private static readonly WUIEngineColor SolidPink = new WUIEngineColor(137, 56, 67, 255);
        private static readonly WUIEngineColor SonicSilver = new WUIEngineColor(117, 117, 117, 255);
        private static readonly WUIEngineColor SpartanCrimson = new WUIEngineColor(158, 19, 22, 255);
        private static readonly WUIEngineColor SpaceCadet = new WUIEngineColor(29, 41, 81, 255);
        private static readonly WUIEngineColor SpanishBistre = new WUIEngineColor(128, 117, 50, 255);
        private static readonly WUIEngineColor SpanishBlue = new WUIEngineColor(0, 112, 184, 255);
        private static readonly WUIEngineColor SpanishCarmine = new WUIEngineColor(209, 0, 71, 255);
        private static readonly WUIEngineColor SpanishCrimson = new WUIEngineColor(229, 26, 76, 255);
        private static readonly WUIEngineColor SpanishGray = new WUIEngineColor(152, 152, 152, 255);
        private static readonly WUIEngineColor SpanishGreen = new WUIEngineColor(0, 145, 80, 255);
        private static readonly WUIEngineColor SpanishOrange = new WUIEngineColor(232, 97, 0, 255);
        private static readonly WUIEngineColor SpanishPink = new WUIEngineColor(247, 191, 190, 255);
        private static readonly WUIEngineColor SpanishPurple = new WUIEngineColor(102, 3, 60, 255);
        private static readonly WUIEngineColor SpanishRed = new WUIEngineColor(230, 0, 38, 255);
        private static readonly WUIEngineColor SpanishViolet = new WUIEngineColor(76, 40, 130, 255);
        private static readonly WUIEngineColor SpanishViridian = new WUIEngineColor(0, 127, 92, 255);
        private static readonly WUIEngineColor SpanishYellow = new WUIEngineColor(246, 181, 17, 255);
        private static readonly WUIEngineColor SpicyMix = new WUIEngineColor(139, 95, 77, 255);
        private static readonly WUIEngineColor SpiroDiscoBall = new WUIEngineColor(15, 192, 252, 255);
        private static readonly WUIEngineColor SpringBud = new WUIEngineColor(167, 252, 0, 255);
        private static readonly WUIEngineColor SpringFrost = new WUIEngineColor(135, 255, 42, 255);
        private static readonly WUIEngineColor StarCommandBlue = new WUIEngineColor(0, 123, 184, 255);
        private static readonly WUIEngineColor SteelBlue = new WUIEngineColor(70, 130, 180, 255);
        private static readonly WUIEngineColor SteelPink = new WUIEngineColor(204, 51, 204, 255);
        private static readonly WUIEngineColor SteelTeal = new WUIEngineColor(95, 138, 139, 255);
        private static readonly WUIEngineColor Stormcloud = new WUIEngineColor(79, 102, 106, 255);
        private static readonly WUIEngineColor Straw = new WUIEngineColor(228, 217, 111, 255);
        private static readonly WUIEngineColor Strawberry = new WUIEngineColor(252, 90, 141, 255);
        private static readonly WUIEngineColor SugarPlum = new WUIEngineColor(145, 78, 117, 255);
        private static readonly WUIEngineColor SunburntCyclops = new WUIEngineColor(255, 64, 76, 255);
        private static readonly WUIEngineColor Sunglow = new WUIEngineColor(255, 204, 51, 255);
        private static readonly WUIEngineColor Sunny = new WUIEngineColor(242, 242, 122, 255);
        private static readonly WUIEngineColor Sunray = new WUIEngineColor(227, 171, 87, 255);
        private static readonly WUIEngineColor SunsetOrange = new WUIEngineColor(253, 94, 83, 255);
        private static readonly WUIEngineColor SuperPink = new WUIEngineColor(207, 107, 169, 255);
        private static readonly WUIEngineColor SweetBrown = new WUIEngineColor(168, 55, 49, 255);
        private static readonly WUIEngineColor Tan = new WUIEngineColor(210, 180, 140, 255);
        private static readonly WUIEngineColor Tangelo = new WUIEngineColor(249, 77, 0, 255);
        private static readonly WUIEngineColor Tangerine = new WUIEngineColor(242, 133, 0, 255);
        private static readonly WUIEngineColor TartOrange = new WUIEngineColor(251, 77, 70, 255);
        private static readonly WUIEngineColor TaupeGray = new WUIEngineColor(139, 133, 137, 255);
        private static readonly WUIEngineColor TeaGreen = new WUIEngineColor(208, 240, 192, 255);
        private static readonly WUIEngineColor Teal = new WUIEngineColor(0, 128, 128, 255);
        private static readonly WUIEngineColor TealBlue = new WUIEngineColor(54, 117, 136, 255);
        private static readonly WUIEngineColor TealDeer = new WUIEngineColor(153, 230, 179, 255);
        private static readonly WUIEngineColor TealGreen = new WUIEngineColor(0, 130, 127, 255);
        private static readonly WUIEngineColor Telemagenta = new WUIEngineColor(207, 52, 118, 255);
        private static readonly WUIEngineColor Temptress = new WUIEngineColor(60, 33, 38, 255);
        private static readonly WUIEngineColor TennéTawny = new WUIEngineColor(205, 87, 0, 255);
        private static readonly WUIEngineColor TerraCotta = new WUIEngineColor(226, 114, 91, 255);
        private static readonly WUIEngineColor Thistle = new WUIEngineColor(216, 191, 216, 255);
        private static readonly WUIEngineColor TickleMePink = new WUIEngineColor(252, 137, 172, 255);
        private static readonly WUIEngineColor TiffanyBlue = new WUIEngineColor(10, 186, 181, 255);
        private static readonly WUIEngineColor TigersEye = new WUIEngineColor(224, 141, 60, 255);
        private static readonly WUIEngineColor Timberwolf = new WUIEngineColor(219, 215, 210, 255);
        private static readonly WUIEngineColor Titanium = new WUIEngineColor(135, 134, 129, 255);
        private static readonly WUIEngineColor TitaniumYellow = new WUIEngineColor(238, 230, 0, 255);
        private static readonly WUIEngineColor Tomato = new WUIEngineColor(255, 99, 71, 255);
        private static readonly WUIEngineColor Toolbox = new WUIEngineColor(116, 108, 192, 255);
        private static readonly WUIEngineColor Topaz = new WUIEngineColor(255, 200, 124, 255);
        private static readonly WUIEngineColor TropicalRainForest = new WUIEngineColor(0, 117, 94, 255);
        private static readonly WUIEngineColor TropicalViolet = new WUIEngineColor(205, 164, 222, 255);
        private static readonly WUIEngineColor TrueBlue = new WUIEngineColor(0, 115, 207, 255);
        private static readonly WUIEngineColor TuftsBlue = new WUIEngineColor(62, 142, 222, 255);
        private static readonly WUIEngineColor Tulip = new WUIEngineColor(255, 135, 141, 255);
        private static readonly WUIEngineColor Tumbleweed = new WUIEngineColor(222, 170, 136, 255);
        private static readonly WUIEngineColor TurkishRose = new WUIEngineColor(181, 114, 129, 255);
        private static readonly WUIEngineColor Turquoise = new WUIEngineColor(64, 224, 208, 255);
        private static readonly WUIEngineColor TurquoiseBlue = new WUIEngineColor(0, 255, 239, 255);
        private static readonly WUIEngineColor TurquoiseGreen = new WUIEngineColor(160, 214, 180, 255);
        private static readonly WUIEngineColor TurquoiseSurf = new WUIEngineColor(0, 197, 205, 255);
        private static readonly WUIEngineColor TuscanRed = new WUIEngineColor(124, 72, 72, 255);
        private static readonly WUIEngineColor Tuscany = new WUIEngineColor(192, 153, 153, 255);
        private static readonly WUIEngineColor TwilightLavender = new WUIEngineColor(138, 73, 107, 255);
        private static readonly WUIEngineColor UABlue = new WUIEngineColor(0, 51, 170, 255);
        private static readonly WUIEngineColor UARed = new WUIEngineColor(217, 0, 76, 255);
        private static readonly WUIEngineColor Ube = new WUIEngineColor(136, 120, 195, 255);
        private static readonly WUIEngineColor UCLABlue = new WUIEngineColor(83, 104, 149, 255);
        private static readonly WUIEngineColor UCLAGold = new WUIEngineColor(255, 179, 0, 255);
        private static readonly WUIEngineColor UERed = new WUIEngineColor(186, 0, 1, 255);
        private static readonly WUIEngineColor UFOGreen = new WUIEngineColor(60, 208, 112, 255);
        private static readonly WUIEngineColor Ultramarine = new WUIEngineColor(18, 10, 143, 255);
        private static readonly WUIEngineColor UltramarineBlue = new WUIEngineColor(65, 102, 245, 255);
        private static readonly WUIEngineColor UltraRed = new WUIEngineColor(252, 108, 133, 255);
        private static readonly WUIEngineColor Umber = new WUIEngineColor(99, 81, 71, 255);
        private static readonly WUIEngineColor UnbleachedSilk = new WUIEngineColor(255, 221, 202, 255);
        private static readonly WUIEngineColor UnitedNationsBlue = new WUIEngineColor(91, 146, 229, 255);
        private static readonly WUIEngineColor UniversityOfCaliforniaGold = new WUIEngineColor(183, 135, 39, 255);
        private static readonly WUIEngineColor UniversityOfTennesseeOrange = new WUIEngineColor(247, 127, 0, 255);
        private static readonly WUIEngineColor UPMaroon = new WUIEngineColor(123, 17, 19, 255);
        private static readonly WUIEngineColor UpsdellRed = new WUIEngineColor(174, 32, 41, 255);
        private static readonly WUIEngineColor Urobilin = new WUIEngineColor(225, 173, 33, 255);
        private static readonly WUIEngineColor USAFABlue = new WUIEngineColor(0, 79, 152, 255);
        private static readonly WUIEngineColor UtahCrimson = new WUIEngineColor(211, 0, 63, 255);
        private static readonly WUIEngineColor VampireBlack = new WUIEngineColor(8, 8, 8, 255);
        private static readonly WUIEngineColor VanDykeBrown = new WUIEngineColor(102, 66, 40, 255);
        private static readonly WUIEngineColor VanillaIce = new WUIEngineColor(243, 143, 169, 255);
        private static readonly WUIEngineColor VegasGold = new WUIEngineColor(197, 179, 88, 255);
        private static readonly WUIEngineColor VenetianRed = new WUIEngineColor(200, 8, 21, 255);
        private static readonly WUIEngineColor Verdigris = new WUIEngineColor(67, 179, 174, 255);
        private static readonly WUIEngineColor Vermilion = new WUIEngineColor(217, 56, 30, 255);
        private static readonly WUIEngineColor VerseGreen = new WUIEngineColor(24, 136, 13, 255);
        private static readonly WUIEngineColor VeryLightAzure = new WUIEngineColor(116, 187, 251, 255);
        private static readonly WUIEngineColor VeryLightBlue = new WUIEngineColor(102, 102, 255, 255);
        private static readonly WUIEngineColor VeryLightMalachiteGreen = new WUIEngineColor(100, 233, 134, 255);
        private static readonly WUIEngineColor VeryLightTangelo = new WUIEngineColor(255, 176, 119, 255);
        private static readonly WUIEngineColor VeryPaleOrange = new WUIEngineColor(255, 223, 191, 255);
        private static readonly WUIEngineColor VeryPaleYellow = new WUIEngineColor(255, 255, 191, 255);
        private static readonly WUIEngineColor VioletColorWheel = new WUIEngineColor(127, 0, 255, 255);
        private static readonly WUIEngineColor VioletCrayola = new WUIEngineColor(150, 61, 127, 255);
        private static readonly WUIEngineColor VioletRYB = new WUIEngineColor(134, 1, 175, 255);
        private static readonly WUIEngineColor VioletBlue = new WUIEngineColor(50, 74, 178, 255);
        private static readonly WUIEngineColor VioletRed = new WUIEngineColor(247, 83, 148, 255);
        private static readonly WUIEngineColor ViolinBrown = new WUIEngineColor(103, 68, 3, 255);
        private static readonly WUIEngineColor ViridianGreen = new WUIEngineColor(0, 150, 152, 255);
        private static readonly WUIEngineColor VistaBlue = new WUIEngineColor(124, 158, 217, 255);
        private static readonly WUIEngineColor VividAuburn = new WUIEngineColor(146, 39, 36, 255);
        private static readonly WUIEngineColor VividBurgundy = new WUIEngineColor(159, 29, 53, 255);
        private static readonly WUIEngineColor VividCerise = new WUIEngineColor(218, 29, 129, 255);
        private static readonly WUIEngineColor VividCerulean = new WUIEngineColor(0, 170, 238, 255);
        private static readonly WUIEngineColor VividCrimson = new WUIEngineColor(204, 0, 51, 255);
        private static readonly WUIEngineColor VividGamboge = new WUIEngineColor(255, 153, 0, 255);
        private static readonly WUIEngineColor VividLimeGreen = new WUIEngineColor(166, 214, 8, 255);
        private static readonly WUIEngineColor VividMalachite = new WUIEngineColor(0, 204, 51, 255);
        private static readonly WUIEngineColor VividMulberry = new WUIEngineColor(184, 12, 227, 255);
        private static readonly WUIEngineColor VividOrange = new WUIEngineColor(255, 95, 0, 255);
        private static readonly WUIEngineColor VividOrangePeel = new WUIEngineColor(255, 160, 0, 255);
        private static readonly WUIEngineColor VividOrchid = new WUIEngineColor(204, 0, 255, 255);
        private static readonly WUIEngineColor VividRaspberry = new WUIEngineColor(255, 0, 108, 255);
        private static readonly WUIEngineColor VividRed = new WUIEngineColor(247, 13, 26, 255);
        private static readonly WUIEngineColor VividRedTangelo = new WUIEngineColor(223, 97, 36, 255);
        private static readonly WUIEngineColor VividSkyBlue = new WUIEngineColor(0, 204, 255, 255);
        private static readonly WUIEngineColor VividTangelo = new WUIEngineColor(240, 116, 39, 255);
        private static readonly WUIEngineColor VividTangerine = new WUIEngineColor(255, 160, 137, 255);
        private static readonly WUIEngineColor VividVermilion = new WUIEngineColor(229, 96, 36, 255);
        private static readonly WUIEngineColor VividViolet = new WUIEngineColor(159, 0, 255, 255);
        private static readonly WUIEngineColor VividYellow = new WUIEngineColor(255, 227, 2, 255);
        private static readonly WUIEngineColor Volt = new WUIEngineColor(205, 255, 0, 255);
        private static readonly WUIEngineColor WageningenGreen = new WUIEngineColor(52, 178, 51, 255);
        private static readonly WUIEngineColor WarmBlack = new WUIEngineColor(0, 66, 66, 255);
        private static readonly WUIEngineColor Watermelon = new WUIEngineColor(240, 92, 133, 255);
        private static readonly WUIEngineColor WatermelonRed = new WUIEngineColor(190, 65, 71, 255);
        private static readonly WUIEngineColor Waterspout = new WUIEngineColor(164, 244, 249, 255);
        private static readonly WUIEngineColor WeldonBlue = new WUIEngineColor(124, 152, 171, 255);
        private static readonly WUIEngineColor Wenge = new WUIEngineColor(100, 84, 82, 255);
        private static readonly WUIEngineColor Wheat = new WUIEngineColor(245, 222, 179, 255);
        private static readonly WUIEngineColor White = new WUIEngineColor(255, 255, 255, 255);
        private static readonly WUIEngineColor WhiteChocolate = new WUIEngineColor(237, 230, 214, 255);
        private static readonly WUIEngineColor WhiteCoffee = new WUIEngineColor(230, 224, 212, 255);
        private static readonly WUIEngineColor WildBlueYonder = new WUIEngineColor(162, 173, 208, 255);
        private static readonly WUIEngineColor WildOrchid = new WUIEngineColor(212, 112, 162, 255);
        private static readonly WUIEngineColor WildStrawberry = new WUIEngineColor(255, 67, 164, 255);
        private static readonly WUIEngineColor WillpowerOrange = new WUIEngineColor(253, 88, 0, 255);
        private static readonly WUIEngineColor WindsorTan = new WUIEngineColor(167, 85, 2, 255);
        private static readonly WUIEngineColor WineRed = new WUIEngineColor(177, 18, 38, 255);
        private static readonly WUIEngineColor WinterSky = new WUIEngineColor(255, 0, 124, 255);
        private static readonly WUIEngineColor WinterWizard = new WUIEngineColor(160, 230, 255, 255);
        private static readonly WUIEngineColor WintergreenDream = new WUIEngineColor(86, 136, 125, 255);
        private static readonly WUIEngineColor Wisteria = new WUIEngineColor(201, 160, 220, 255);
        private static readonly WUIEngineColor Xanadu = new WUIEngineColor(115, 134, 120, 255);
        private static readonly WUIEngineColor YaleBlue = new WUIEngineColor(15, 77, 146, 255);
        private static readonly WUIEngineColor YankeesBlue = new WUIEngineColor(28, 40, 65, 255);
        private static readonly WUIEngineColor Yellow = new WUIEngineColor(255, 255, 0, 255);
        private static readonly WUIEngineColor YellowCrayola = new WUIEngineColor(252, 232, 131, 255);
        private static readonly WUIEngineColor YellowMunsell = new WUIEngineColor(239, 204, 0, 255);
        private static readonly WUIEngineColor YellowPantone = new WUIEngineColor(254, 223, 0, 255);
        private static readonly WUIEngineColor YellowRYB = new WUIEngineColor(254, 254, 51, 255);
        private static readonly WUIEngineColor YellowGreen = new WUIEngineColor(154, 205, 50, 255);
        private static readonly WUIEngineColor YellowOrange = new WUIEngineColor(255, 174, 66, 255);
        private static readonly WUIEngineColor YellowRose = new WUIEngineColor(255, 240, 0, 255);
        private static readonly WUIEngineColor Zaffre = new WUIEngineColor(0, 20, 168, 255);
        private static readonly WUIEngineColor ZinnwalditeBrown = new WUIEngineColor(44, 22, 8, 255);
        private static readonly WUIEngineColor Zomp = new WUIEngineColor(57, 167, 142, 255);

		private static readonly WUIEngineColor[] colors =
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
