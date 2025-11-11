//This file is part of WUIPlatform Copyright (C) 2024 Jonathan Wahlqvist
//WUIPlatform is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
//This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
//You should have received a copy of the GNU General Public License along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace PREACT.Visualization
{
	public static class FuelModelColors
	{
		private static PREACTColor ERROR_COLOR = new PREACTColor(1.0f, 0, 1.0f);
		public static PREACTColor GetFuelColor(int fuelNumber)
		{
			if (fuelNumber > 0 && fuelNumber <= colors.Length)
			{
				PREACTColor c = colors[fuelNumber - 1];
				return c;
			}
			return ERROR_COLOR;
		}

        private static readonly PREACTColor AbsoluteZero = new PREACTColor(0, 72, 186, 255);
        private static readonly PREACTColor Acajou = new PREACTColor(76, 47, 39, 255);
        private static readonly PREACTColor AcidGreen = new PREACTColor(176, 191, 26, 255);
        private static readonly PREACTColor Aero = new PREACTColor(124, 185, 232, 255);
        private static readonly PREACTColor AeroBlue = new PREACTColor(201, 255, 229, 255);
        private static readonly PREACTColor AfricanViolet = new PREACTColor(178, 132, 190, 255);
        private static readonly PREACTColor AirForceBlueRAF = new PREACTColor(93, 138, 168, 255);
        private static readonly PREACTColor AirForceBlueUSAF = new PREACTColor(0, 48, 143, 255);
        private static readonly PREACTColor AirSuperiorityBlue = new PREACTColor(114, 160, 193, 255);
        private static readonly PREACTColor AlabamaCrimson = new PREACTColor(175, 48, 42, 255);
        private static readonly PREACTColor Alabaster = new PREACTColor(242, 240, 230, 255);
        private static readonly PREACTColor AliceBlue = new PREACTColor(240, 248, 255, 255);
        private static readonly PREACTColor AlienArmpit = new PREACTColor(132, 222, 2, 255);
        private static readonly PREACTColor AlizarinCrimson = new PREACTColor(227, 38, 54, 255);
        private static readonly PREACTColor AlloyOrange = new PREACTColor(196, 98, 16, 255);
        private static readonly PREACTColor Almond = new PREACTColor(239, 222, 205, 255);
        private static readonly PREACTColor Amaranth = new PREACTColor(229, 43, 80, 255);
        private static readonly PREACTColor AmaranthDeepPurple = new PREACTColor(159, 43, 104, 255);
        private static readonly PREACTColor AmaranthPink = new PREACTColor(241, 156, 187, 255);
        private static readonly PREACTColor AmaranthPurple = new PREACTColor(171, 39, 79, 255);
        private static readonly PREACTColor AmaranthRed = new PREACTColor(211, 33, 45, 255);
        private static readonly PREACTColor Amazon = new PREACTColor(59, 122, 87, 255);
        private static readonly PREACTColor Amazonite = new PREACTColor(0, 196, 176, 255);
        private static readonly PREACTColor Amber = new PREACTColor(255, 191, 0, 255);
        private static readonly PREACTColor AmberSAEECE = new PREACTColor(255, 126, 0, 255);
        private static readonly PREACTColor AmericanBlue = new PREACTColor(59, 59, 109, 255);
        private static readonly PREACTColor AmericanBrown = new PREACTColor(128, 64, 64, 255);
        private static readonly PREACTColor AmericanGold = new PREACTColor(211, 175, 55, 255);
        private static readonly PREACTColor AmericanGreen = new PREACTColor(52, 179, 52, 255);
        private static readonly PREACTColor AmericanOrange = new PREACTColor(255, 139, 0, 255);
        private static readonly PREACTColor AmericanPink = new PREACTColor(255, 152, 153, 255);
        private static readonly PREACTColor AmericanPurple = new PREACTColor(67, 28, 83, 255);
        private static readonly PREACTColor AmericanRed = new PREACTColor(179, 33, 52, 255);
        private static readonly PREACTColor AmericanRose = new PREACTColor(255, 3, 62, 255);
        private static readonly PREACTColor AmericanSilver = new PREACTColor(207, 207, 207, 255);
        private static readonly PREACTColor AmericanViolet = new PREACTColor(85, 27, 140, 255);
        private static readonly PREACTColor AmericanYellow = new PREACTColor(242, 180, 0, 255);
        private static readonly PREACTColor Amethyst = new PREACTColor(153, 102, 204, 255);
        private static readonly PREACTColor AndroidGreen = new PREACTColor(164, 198, 57, 255);
        private static readonly PREACTColor AntiFlashWhite = new PREACTColor(242, 243, 244, 255);
        private static readonly PREACTColor AntiqueBrass = new PREACTColor(205, 149, 117, 255);
        private static readonly PREACTColor AntiqueBronze = new PREACTColor(102, 93, 30, 255);
        private static readonly PREACTColor AntiqueFuchsia = new PREACTColor(145, 92, 131, 255);
        private static readonly PREACTColor AntiqueRuby = new PREACTColor(132, 27, 45, 255);
        private static readonly PREACTColor AntiqueWhite = new PREACTColor(250, 235, 215, 255);
        private static readonly PREACTColor AoEnglish = new PREACTColor(0, 128, 0, 255);
        private static readonly PREACTColor Apple = new PREACTColor(102, 180, 71, 255);
        private static readonly PREACTColor AppleGreen = new PREACTColor(141, 182, 0, 255);
        private static readonly PREACTColor Apricot = new PREACTColor(251, 206, 177, 255);
        private static readonly PREACTColor Aqua = new PREACTColor(0, 255, 255, 255);
        private static readonly PREACTColor Aquamarine = new PREACTColor(127, 255, 212, 255);
        private static readonly PREACTColor ArcticLime = new PREACTColor(208, 255, 20, 255);
        private static readonly PREACTColor ArmyGreen = new PREACTColor(75, 83, 32, 255);
        private static readonly PREACTColor Arsenic = new PREACTColor(59, 68, 75, 255);
        private static readonly PREACTColor Artichoke = new PREACTColor(143, 151, 121, 255);
        private static readonly PREACTColor ArylideYellow = new PREACTColor(233, 214, 107, 255);
        private static readonly PREACTColor AshGray = new PREACTColor(178, 190, 181, 255);
        private static readonly PREACTColor Asparagus = new PREACTColor(135, 169, 107, 255);
        private static readonly PREACTColor AteneoBlue = new PREACTColor(0, 58, 108, 255);
        private static readonly PREACTColor AtomicTangerine = new PREACTColor(255, 153, 102, 255);
        private static readonly PREACTColor Auburn = new PREACTColor(165, 42, 42, 255);
        private static readonly PREACTColor Aureolin = new PREACTColor(253, 238, 0, 255);
        private static readonly PREACTColor Aurometalsaurus = new PREACTColor(110, 127, 128, 255);
        private static readonly PREACTColor Avocado = new PREACTColor(86, 130, 3, 255);
        private static readonly PREACTColor Awesome = new PREACTColor(255, 32, 82, 255);
        private static readonly PREACTColor Axolotl = new PREACTColor(99, 119, 91, 255);
        private static readonly PREACTColor AztecGold = new PREACTColor(195, 153, 83, 255);
        private static readonly PREACTColor Azure = new PREACTColor(0, 127, 255, 255);
        private static readonly PREACTColor AzureWebColor = new PREACTColor(240, 255, 255, 255);
        private static readonly PREACTColor AzureishWhite = new PREACTColor(219, 233, 244, 255);
        private static readonly PREACTColor BabyBlue = new PREACTColor(137, 207, 240, 255);
        private static readonly PREACTColor BabyBlueEyes = new PREACTColor(161, 202, 241, 255);
        private static readonly PREACTColor BabyPink = new PREACTColor(244, 194, 194, 255);
        private static readonly PREACTColor BabyPowder = new PREACTColor(254, 254, 250, 255);
        private static readonly PREACTColor BakerMillerPink = new PREACTColor(255, 145, 175, 255);
        private static readonly PREACTColor BallBlue = new PREACTColor(33, 171, 205, 255);
        private static readonly PREACTColor BananaMania = new PREACTColor(250, 231, 181, 255);
        private static readonly PREACTColor BananaYellow = new PREACTColor(255, 225, 53, 255);
        private static readonly PREACTColor BangladeshGreen = new PREACTColor(0, 106, 78, 255);
        private static readonly PREACTColor BarbiePink = new PREACTColor(224, 33, 138, 255);
        private static readonly PREACTColor BarnRed = new PREACTColor(124, 10, 2, 255);
        private static readonly PREACTColor BatteryChargedBlue = new PREACTColor(29, 172, 214, 255);
        private static readonly PREACTColor BattleshipGrey = new PREACTColor(132, 132, 130, 255);
        private static readonly PREACTColor Bazaar = new PREACTColor(152, 119, 123, 255);
        private static readonly PREACTColor BeauBlue = new PREACTColor(188, 212, 230, 255);
        private static readonly PREACTColor Beaver = new PREACTColor(159, 129, 112, 255);
        private static readonly PREACTColor Begonia = new PREACTColor(250, 110, 121, 255);
        private static readonly PREACTColor Beige = new PREACTColor(245, 245, 220, 255);
        private static readonly PREACTColor BdazzledBlue = new PREACTColor(46, 88, 148, 255);
        private static readonly PREACTColor BigDipORuby = new PREACTColor(156, 37, 66, 255);
        private static readonly PREACTColor BigFootFeet = new PREACTColor(232, 142, 90, 255);
        private static readonly PREACTColor Bisque = new PREACTColor(255, 228, 196, 255);
        private static readonly PREACTColor Bistre = new PREACTColor(61, 43, 31, 255);
        private static readonly PREACTColor BistreBrown = new PREACTColor(150, 113, 23, 255);
        private static readonly PREACTColor BitterLemon = new PREACTColor(202, 224, 13, 255);
        private static readonly PREACTColor BitterLime = new PREACTColor(191, 255, 0, 255);
        private static readonly PREACTColor Bittersweet = new PREACTColor(254, 111, 94, 255);
        private static readonly PREACTColor BittersweetShimmer = new PREACTColor(191, 79, 81, 255);
        private static readonly PREACTColor Black = new PREACTColor(0, 0, 0, 255);
        private static readonly PREACTColor BlackBean = new PREACTColor(61, 12, 2, 255);
        private static readonly PREACTColor BlackChocolate = new PREACTColor(27, 24, 17, 255);
        private static readonly PREACTColor BlackCoffee = new PREACTColor(59, 47, 47, 255);
        private static readonly PREACTColor BlackCoral = new PREACTColor(84, 98, 111, 255);
        private static readonly PREACTColor BlackLeatherJacket = new PREACTColor(37, 53, 41, 255);
        private static readonly PREACTColor BlackOlive = new PREACTColor(59, 60, 54, 255);
        private static readonly PREACTColor Blackberry = new PREACTColor(143, 89, 115, 255);
        private static readonly PREACTColor BlackShadows = new PREACTColor(191, 175, 178, 255);
        private static readonly PREACTColor BlanchedAlmond = new PREACTColor(255, 235, 205, 255);
        private static readonly PREACTColor BlastOffBronze = new PREACTColor(165, 113, 100, 255);
        private static readonly PREACTColor BleuDeFrance = new PREACTColor(49, 140, 231, 255);
        private static readonly PREACTColor BlizzardBlue = new PREACTColor(172, 229, 238, 255);
        private static readonly PREACTColor Blond = new PREACTColor(250, 240, 190, 255);
        private static readonly PREACTColor BloodOrange = new PREACTColor(210, 0, 27, 255);
        private static readonly PREACTColor BloodRed = new PREACTColor(102, 0, 0, 255);
        private static readonly PREACTColor Blue = new PREACTColor(0, 0, 255, 255);
        private static readonly PREACTColor BlueCrayola = new PREACTColor(31, 117, 254, 255);
        private static readonly PREACTColor BlueMunsell = new PREACTColor(0, 147, 175, 255);
        private static readonly PREACTColor BlueNCS = new PREACTColor(0, 135, 189, 255);
        private static readonly PREACTColor BluePantone = new PREACTColor(0, 24, 168, 255);
        private static readonly PREACTColor BluePigment = new PREACTColor(51, 51, 153, 255);
        private static readonly PREACTColor BlueRYB = new PREACTColor(2, 71, 254, 255);
        private static readonly PREACTColor BlueBell = new PREACTColor(162, 162, 208, 255);
        private static readonly PREACTColor BlueBolt = new PREACTColor(0, 185, 251, 255);
        private static readonly PREACTColor BlueGray = new PREACTColor(102, 153, 204, 255);
        private static readonly PREACTColor BlueGreen = new PREACTColor(13, 152, 186, 255);
        private static readonly PREACTColor BlueJeans = new PREACTColor(93, 173, 236, 255);
        private static readonly PREACTColor BlueMagentaViolet = new PREACTColor(85, 53, 146, 255);
        private static readonly PREACTColor BlueSapphire = new PREACTColor(18, 97, 128, 255);
        private static readonly PREACTColor BlueViolet = new PREACTColor(138, 43, 226, 255);
        private static readonly PREACTColor BlueYonder = new PREACTColor(80, 114, 167, 255);
        private static readonly PREACTColor Blueberry = new PREACTColor(79, 134, 247, 255);
        private static readonly PREACTColor Bluebonnet = new PREACTColor(28, 28, 240, 255);
        private static readonly PREACTColor Blush = new PREACTColor(222, 93, 131, 255);
        private static readonly PREACTColor Bole = new PREACTColor(121, 68, 59, 255);
        private static readonly PREACTColor BondiBlue = new PREACTColor(0, 149, 182, 255);
        private static readonly PREACTColor Bone = new PREACTColor(227, 218, 201, 255);
        private static readonly PREACTColor BoogerBuster = new PREACTColor(221, 226, 106, 255);
        private static readonly PREACTColor BostonUniversityRed = new PREACTColor(204, 0, 0, 255);
        private static readonly PREACTColor Boysenberry = new PREACTColor(135, 50, 96, 255);
        private static readonly PREACTColor BrandeisBlue = new PREACTColor(0, 112, 255, 255);
        private static readonly PREACTColor Brass = new PREACTColor(181, 166, 66, 255);
        private static readonly PREACTColor BrickRed = new PREACTColor(203, 65, 84, 255);
        private static readonly PREACTColor BrightGray = new PREACTColor(235, 236, 240, 255);
        private static readonly PREACTColor BrightGreen = new PREACTColor(102, 255, 0, 255);
        private static readonly PREACTColor BrightLavender = new PREACTColor(191, 148, 228, 255);
        private static readonly PREACTColor BrightLilac = new PREACTColor(216, 145, 239, 255);
        private static readonly PREACTColor BrightMaroon = new PREACTColor(195, 33, 72, 255);
        private static readonly PREACTColor BrightNavyBlue = new PREACTColor(25, 116, 210, 255);
        private static readonly PREACTColor BrightPink = new PREACTColor(255, 0, 127, 255);
        private static readonly PREACTColor BrightTurquoise = new PREACTColor(8, 232, 222, 255);
        private static readonly PREACTColor BrightUbe = new PREACTColor(209, 159, 232, 255);
        private static readonly PREACTColor BrightYellowCrayola = new PREACTColor(255, 170, 29, 255);
        private static readonly PREACTColor BrilliantAzure = new PREACTColor(51, 153, 255, 255);
        private static readonly PREACTColor BrilliantLavender = new PREACTColor(244, 187, 255, 255);
        private static readonly PREACTColor BrilliantRose = new PREACTColor(255, 85, 163, 255);
        private static readonly PREACTColor BrinkPink = new PREACTColor(251, 96, 127, 255);
        private static readonly PREACTColor BritishRacingGreen = new PREACTColor(0, 66, 37, 255);
        private static readonly PREACTColor Bronze = new PREACTColor(136, 84, 11, 255);
        private static readonly PREACTColor Bronze2 = new PREACTColor(205, 127, 50, 255);
        private static readonly PREACTColor BronzeMetallic = new PREACTColor(176, 140, 86, 255);
        private static readonly PREACTColor BronzeYellow = new PREACTColor(115, 112, 0, 255);
        private static readonly PREACTColor Brown = new PREACTColor(153, 51, 0, 255);
        private static readonly PREACTColor BrownCrayola = new PREACTColor(175, 89, 62, 255);
        private static readonly PREACTColor BrownTraditional = new PREACTColor(150, 75, 0, 255);
        private static readonly PREACTColor BrownNose = new PREACTColor(107, 68, 35, 255);
        private static readonly PREACTColor BrownSugar = new PREACTColor(175, 110, 77, 255);
        private static readonly PREACTColor BrownChocolate = new PREACTColor(95, 25, 51, 255);
        private static readonly PREACTColor BrownCoffee = new PREACTColor(74, 44, 42, 255);
        private static readonly PREACTColor BrownYellow = new PREACTColor(204, 153, 102, 255);
        private static readonly PREACTColor BrunswickGreen = new PREACTColor(27, 77, 62, 255);
        private static readonly PREACTColor BubbleGum = new PREACTColor(255, 193, 204, 255);
        private static readonly PREACTColor Bubbles = new PREACTColor(231, 254, 255, 255);
        private static readonly PREACTColor BudGreen = new PREACTColor(123, 182, 97, 255);
        private static readonly PREACTColor Buff = new PREACTColor(240, 220, 130, 255);
        private static readonly PREACTColor BulgarianRose = new PREACTColor(72, 6, 7, 255);
        private static readonly PREACTColor Burgundy = new PREACTColor(128, 0, 32, 255);
        private static readonly PREACTColor Burlywood = new PREACTColor(222, 184, 135, 255);
        private static readonly PREACTColor BurnishedBrown = new PREACTColor(161, 122, 116, 255);
        private static readonly PREACTColor BurntOrange = new PREACTColor(204, 85, 0, 255);
        private static readonly PREACTColor BurntSienna = new PREACTColor(233, 116, 81, 255);
        private static readonly PREACTColor BurntUmber = new PREACTColor(138, 51, 36, 255);
        private static readonly PREACTColor ButtonBlue = new PREACTColor(36, 160, 237, 255);
        private static readonly PREACTColor Byzantine = new PREACTColor(189, 51, 164, 255);
        private static readonly PREACTColor Byzantium = new PREACTColor(112, 41, 99, 255);
        private static readonly PREACTColor Cadet = new PREACTColor(83, 104, 114, 255);
        private static readonly PREACTColor CadetBlue = new PREACTColor(95, 158, 160, 255);
        private static readonly PREACTColor CadetGrey = new PREACTColor(145, 163, 176, 255);
        private static readonly PREACTColor CadmiumBlue = new PREACTColor(10, 17, 146, 255);
        private static readonly PREACTColor CadmiumGreen = new PREACTColor(0, 107, 60, 255);
        private static readonly PREACTColor CadmiumOrange = new PREACTColor(237, 135, 45, 255);
        private static readonly PREACTColor CadmiumPurple = new PREACTColor(182, 12, 38, 255);
        private static readonly PREACTColor CadmiumRed = new PREACTColor(227, 0, 34, 255);
        private static readonly PREACTColor CadmiumYellow = new PREACTColor(255, 246, 0, 255);
        private static readonly PREACTColor CadmiumViolet = new PREACTColor(127, 62, 152, 255);
        private static readonly PREACTColor CaféAuLait = new PREACTColor(166, 123, 91, 255);
        private static readonly PREACTColor CaféNoir = new PREACTColor(75, 54, 33, 255);
        private static readonly PREACTColor CalPolyPomonaGreen = new PREACTColor(30, 77, 43, 255);
        private static readonly PREACTColor Calamansi = new PREACTColor(252, 255, 164, 255);
        private static readonly PREACTColor CambridgeBlue = new PREACTColor(163, 193, 173, 255);
        private static readonly PREACTColor Camel = new PREACTColor(193, 154, 107, 255);
        private static readonly PREACTColor CameoPink = new PREACTColor(239, 187, 204, 255);
        private static readonly PREACTColor CamouflageGreen = new PREACTColor(120, 134, 107, 255);
        private static readonly PREACTColor Canary = new PREACTColor(255, 255, 153, 255);
        private static readonly PREACTColor CanaryYellow = new PREACTColor(255, 239, 0, 255);
        private static readonly PREACTColor CandyAppleRed = new PREACTColor(255, 8, 0, 255);
        private static readonly PREACTColor CandyPink = new PREACTColor(228, 113, 122, 255);
        private static readonly PREACTColor Capri = new PREACTColor(0, 191, 255, 255);
        private static readonly PREACTColor CaputMortuum = new PREACTColor(89, 39, 32, 255);
        private static readonly PREACTColor Caramel = new PREACTColor(255, 213, 154, 255);
        private static readonly PREACTColor Cardinal = new PREACTColor(196, 30, 58, 255);
        private static readonly PREACTColor CaribbeanGreen = new PREACTColor(0, 204, 153, 255);
        private static readonly PREACTColor Carmine = new PREACTColor(150, 0, 24, 255);
        private static readonly PREACTColor CarmineMP = new PREACTColor(215, 0, 64, 255);
        private static readonly PREACTColor CarminePink = new PREACTColor(235, 76, 66, 255);
        private static readonly PREACTColor CarmineRed = new PREACTColor(255, 0, 56, 255);
        private static readonly PREACTColor CarnationPink = new PREACTColor(255, 166, 201, 255);
        private static readonly PREACTColor Carnelian = new PREACTColor(179, 27, 27, 255);
        private static readonly PREACTColor CarolinaBlue = new PREACTColor(86, 160, 211, 255);
        private static readonly PREACTColor CarrotOrange = new PREACTColor(237, 145, 33, 255);
        private static readonly PREACTColor CastletonGreen = new PREACTColor(0, 86, 63, 255);
        private static readonly PREACTColor CatalinaBlue = new PREACTColor(6, 42, 120, 255);
        private static readonly PREACTColor Catawba = new PREACTColor(112, 54, 66, 255);
        private static readonly PREACTColor CedarChest = new PREACTColor(201, 90, 73, 255);
        private static readonly PREACTColor Ceil = new PREACTColor(146, 161, 207, 255);
        private static readonly PREACTColor Celadon = new PREACTColor(172, 225, 175, 255);
        private static readonly PREACTColor CeladonBlue = new PREACTColor(0, 123, 167, 255);
        private static readonly PREACTColor CeladonGreen = new PREACTColor(47, 132, 124, 255);
        private static readonly PREACTColor Celeste = new PREACTColor(178, 255, 255, 255);
        private static readonly PREACTColor CelestialBlue = new PREACTColor(73, 151, 208, 255);
        private static readonly PREACTColor Cerise = new PREACTColor(222, 49, 99, 255);
        private static readonly PREACTColor CerisePink = new PREACTColor(236, 59, 131, 255);
        private static readonly PREACTColor CeruleanBlue = new PREACTColor(42, 82, 190, 255);
        private static readonly PREACTColor CeruleanFrost = new PREACTColor(109, 155, 195, 255);
        private static readonly PREACTColor CGBlue = new PREACTColor(0, 122, 165, 255);
        private static readonly PREACTColor CGRed = new PREACTColor(224, 60, 49, 255);
        private static readonly PREACTColor Chamoisee = new PREACTColor(160, 120, 90, 255);
        private static readonly PREACTColor Champagne = new PREACTColor(247, 231, 206, 255);
        private static readonly PREACTColor ChampagnePink = new PREACTColor(241, 221, 207, 255);
        private static readonly PREACTColor Charcoal = new PREACTColor(54, 69, 79, 255);
        private static readonly PREACTColor CharlestonGreen = new PREACTColor(35, 43, 43, 255);
        private static readonly PREACTColor Charm = new PREACTColor(208, 116, 139, 255);
        private static readonly PREACTColor CharmPink = new PREACTColor(230, 143, 172, 255);
        private static readonly PREACTColor ChartreuseTraditional = new PREACTColor(223, 255, 0, 255);
        private static readonly PREACTColor ChartreuseWeb = new PREACTColor(127, 255, 0, 255);
        private static readonly PREACTColor Cheese = new PREACTColor(255, 166, 0, 255);
        private static readonly PREACTColor CherryBlossomPink = new PREACTColor(255, 183, 197, 255);
        private static readonly PREACTColor Chestnut = new PREACTColor(149, 69, 53, 255);
        private static readonly PREACTColor ChinaPink = new PREACTColor(222, 111, 161, 255);
        private static readonly PREACTColor ChinaRose = new PREACTColor(168, 81, 110, 255);
        private static readonly PREACTColor ChineseBlack = new PREACTColor(20, 20, 20, 255);
        private static readonly PREACTColor ChineseBlue = new PREACTColor(54, 81, 148, 255);
        private static readonly PREACTColor ChineseBronze = new PREACTColor(205, 128, 50, 255);
        private static readonly PREACTColor ChineseBrown = new PREACTColor(170, 56, 30, 255);
        private static readonly PREACTColor ChineseGreen = new PREACTColor(208, 219, 97, 255);
        private static readonly PREACTColor ChineseGold = new PREACTColor(204, 153, 0, 255);
        private static readonly PREACTColor ChineseOrange = new PREACTColor(243, 112, 66, 255);
        private static readonly PREACTColor ChinesePink = new PREACTColor(222, 112, 161, 255);
        private static readonly PREACTColor ChinesePurple = new PREACTColor(114, 11, 152, 255);
        private static readonly PREACTColor ChineseRed = new PREACTColor(205, 7, 30, 255);
        private static readonly PREACTColor ChineseSilver = new PREACTColor(204, 204, 204, 255);
        private static readonly PREACTColor ChineseViolet = new PREACTColor(133, 96, 136, 255);
        private static readonly PREACTColor ChineseWhite = new PREACTColor(226, 229, 222, 255);
        private static readonly PREACTColor ChineseYellow = new PREACTColor(255, 178, 0, 255);
        private static readonly PREACTColor ChlorophyllGreen = new PREACTColor(74, 255, 0, 255);
        private static readonly PREACTColor ChocolateKisses = new PREACTColor(60, 20, 33, 255);
        private static readonly PREACTColor ChocolateTraditional = new PREACTColor(123, 63, 0, 255);
        private static readonly PREACTColor ChocolateWeb = new PREACTColor(210, 105, 30, 255);
        private static readonly PREACTColor ChristmasBlue = new PREACTColor(42, 143, 189, 255);
        private static readonly PREACTColor ChristmasBrown = new PREACTColor(93, 43, 44, 255);
        private static readonly PREACTColor ChristmasBrown2 = new PREACTColor(76, 31, 2, 255);
        private static readonly PREACTColor ChristmasGreen = new PREACTColor(60, 141, 13, 255);
        private static readonly PREACTColor ChristmasGreen2 = new PREACTColor(0, 117, 2, 255);
        private static readonly PREACTColor ChristmasGold = new PREACTColor(202, 169, 6, 255);
        private static readonly PREACTColor ChristmasOrange = new PREACTColor(255, 102, 0, 255);
        private static readonly PREACTColor ChristmasOrange2 = new PREACTColor(213, 108, 43, 255);
        private static readonly PREACTColor ChristmasPink = new PREACTColor(255, 204, 203, 255);
        private static readonly PREACTColor ChristmasPink2 = new PREACTColor(227, 66, 133, 255);
        private static readonly PREACTColor ChristmasPurple = new PREACTColor(102, 51, 152, 255);
        private static readonly PREACTColor ChristmasPurple2 = new PREACTColor(77, 8, 77, 255);
        private static readonly PREACTColor ChristmasRed = new PREACTColor(170, 1, 20, 255);
        private static readonly PREACTColor ChristmasRed2 = new PREACTColor(176, 27, 46, 255);
        private static readonly PREACTColor ChristmasSilver = new PREACTColor(225, 223, 224, 255);
        private static readonly PREACTColor ChristmasYellow = new PREACTColor(255, 204, 0, 255);
        private static readonly PREACTColor ChristmasYellow2 = new PREACTColor(254, 242, 0, 255);
        private static readonly PREACTColor ChromeYellow = new PREACTColor(255, 167, 0, 255);
        private static readonly PREACTColor Cinereous = new PREACTColor(152, 129, 123, 255);
        private static readonly PREACTColor Cinnabar = new PREACTColor(227, 66, 52, 255);
        private static readonly PREACTColor CinnamonSatin = new PREACTColor(205, 96, 126, 255);
        private static readonly PREACTColor Citrine = new PREACTColor(228, 208, 10, 255);
        private static readonly PREACTColor CitrineBrown = new PREACTColor(147, 55, 9, 255);
        private static readonly PREACTColor Citron = new PREACTColor(158, 169, 31, 255);
        private static readonly PREACTColor Claret = new PREACTColor(127, 23, 52, 255);
        private static readonly PREACTColor ClassicRose = new PREACTColor(251, 204, 231, 255);
        private static readonly PREACTColor CobaltBlue = new PREACTColor(0, 71, 171, 255);
        private static readonly PREACTColor Coconut = new PREACTColor(150, 90, 62, 255);
        private static readonly PREACTColor Coffee = new PREACTColor(111, 78, 55, 255);
        private static readonly PREACTColor Cola = new PREACTColor(60, 48, 36, 255);
        private static readonly PREACTColor ColumbiaBlue = new PREACTColor(196, 216, 226, 255);
        private static readonly PREACTColor Conditioner = new PREACTColor(255, 255, 204, 255);
        private static readonly PREACTColor CongoPink = new PREACTColor(248, 131, 121, 255);
        private static readonly PREACTColor CoolBlack = new PREACTColor(0, 46, 99, 255);
        private static readonly PREACTColor CoolGrey = new PREACTColor(140, 146, 172, 255);
        private static readonly PREACTColor CookiesAndCream = new PREACTColor(238, 224, 177, 255);
        private static readonly PREACTColor Copper = new PREACTColor(184, 115, 51, 255);
        private static readonly PREACTColor CopperCrayola = new PREACTColor(218, 138, 103, 255);
        private static readonly PREACTColor CopperPenny = new PREACTColor(173, 111, 105, 255);
        private static readonly PREACTColor CopperRed = new PREACTColor(203, 109, 81, 255);
        private static readonly PREACTColor CopperRose = new PREACTColor(153, 102, 102, 255);
        private static readonly PREACTColor Coquelicot = new PREACTColor(255, 56, 0, 255);
        private static readonly PREACTColor Coral = new PREACTColor(255, 127, 80, 255);
        private static readonly PREACTColor CoralRed = new PREACTColor(255, 64, 64, 255);
        private static readonly PREACTColor CoralReef = new PREACTColor(253, 124, 110, 255);
        private static readonly PREACTColor Cordovan = new PREACTColor(137, 63, 69, 255);
        private static readonly PREACTColor Corn = new PREACTColor(251, 236, 93, 255);
        private static readonly PREACTColor CornflowerBlue = new PREACTColor(100, 149, 237, 255);
        private static readonly PREACTColor Cornsilk = new PREACTColor(255, 248, 220, 255);
        private static readonly PREACTColor CosmicCobalt = new PREACTColor(46, 45, 136, 255);
        private static readonly PREACTColor CosmicLatte = new PREACTColor(255, 248, 231, 255);
        private static readonly PREACTColor CoyoteBrown = new PREACTColor(129, 97, 60, 255);
        private static readonly PREACTColor CottonCandy = new PREACTColor(255, 188, 217, 255);
        private static readonly PREACTColor Cream = new PREACTColor(255, 253, 208, 255);
        private static readonly PREACTColor Crimson = new PREACTColor(220, 20, 60, 255);
        private static readonly PREACTColor CrimsonGlory = new PREACTColor(190, 0, 50, 255);
        private static readonly PREACTColor CrimsonRed = new PREACTColor(153, 0, 0, 255);
        private static readonly PREACTColor Cultured = new PREACTColor(245, 245, 245, 255);
        private static readonly PREACTColor CyanAzure = new PREACTColor(78, 130, 180, 255);
        private static readonly PREACTColor CyanBlueAzure = new PREACTColor(70, 130, 191, 255);
        private static readonly PREACTColor CyanCobaltBlue = new PREACTColor(40, 88, 156, 255);
        private static readonly PREACTColor CyanCornflowerBlue = new PREACTColor(24, 139, 194, 255);
        private static readonly PREACTColor CyanProcess = new PREACTColor(0, 183, 235, 255);
        private static readonly PREACTColor CyberGrape = new PREACTColor(88, 66, 124, 255);
        private static readonly PREACTColor CyberYellow = new PREACTColor(255, 211, 0, 255);
        private static readonly PREACTColor Cyclamen = new PREACTColor(245, 111, 161, 255);
        private static readonly PREACTColor Daffodil = new PREACTColor(255, 255, 49, 255);
        private static readonly PREACTColor Dandelion = new PREACTColor(240, 225, 48, 255);
        private static readonly PREACTColor DarkBlue = new PREACTColor(0, 0, 139, 255);
        private static readonly PREACTColor DarkBlueGray = new PREACTColor(102, 102, 153, 255);
        private static readonly PREACTColor DarkBronze = new PREACTColor(128, 74, 0, 255);
        private static readonly PREACTColor DarkBrown = new PREACTColor(101, 67, 33, 255);
        private static readonly PREACTColor DarkBrownTangelo = new PREACTColor(136, 101, 78, 255);
        private static readonly PREACTColor DarkByzantium = new PREACTColor(93, 57, 84, 255);
        private static readonly PREACTColor DarkCandyAppleRed = new PREACTColor(164, 0, 0, 255);
        private static readonly PREACTColor DarkCerulean = new PREACTColor(8, 69, 126, 255);
        private static readonly PREACTColor DarkCharcoal = new PREACTColor(51, 51, 51, 255);
        private static readonly PREACTColor DarkChestnut = new PREACTColor(152, 105, 96, 255);
        private static readonly PREACTColor DarkChocolate = new PREACTColor(73, 2, 6, 255);
        private static readonly PREACTColor DarkChocolateHersheys = new PREACTColor(60, 19, 33, 255);
        private static readonly PREACTColor DarkCornflowerBlue = new PREACTColor(38, 66, 139, 255);
        private static readonly PREACTColor DarkCoral = new PREACTColor(205, 91, 69, 255);
        private static readonly PREACTColor DarkCyan = new PREACTColor(0, 139, 139, 255);
        private static readonly PREACTColor DarkElectricBlue = new PREACTColor(83, 104, 120, 255);
        private static readonly PREACTColor DarkGoldenrod = new PREACTColor(184, 134, 11, 255);
        private static readonly PREACTColor DarkGrayX11 = new PREACTColor(169, 169, 169, 255);
        private static readonly PREACTColor DarkGreen = new PREACTColor(1, 50, 32, 255);
        private static readonly PREACTColor DarkGreenX11 = new PREACTColor(0, 100, 0, 255);
        private static readonly PREACTColor DarkGunmetal = new PREACTColor(31, 38, 42, 255);
        private static readonly PREACTColor DarkImperialBlue = new PREACTColor(0, 65, 106, 255);
        private static readonly PREACTColor DarkImperialBlue2 = new PREACTColor(0, 20, 126, 255);
        private static readonly PREACTColor DarkJungleGreen = new PREACTColor(26, 36, 33, 255);
        private static readonly PREACTColor DarkKhaki = new PREACTColor(189, 183, 107, 255);
        private static readonly PREACTColor DarkLava = new PREACTColor(72, 60, 50, 255);
        private static readonly PREACTColor DarkLavender = new PREACTColor(115, 79, 150, 255);
        private static readonly PREACTColor DarkLemonLime = new PREACTColor(139, 190, 27, 255);
        private static readonly PREACTColor DarkLiver = new PREACTColor(83, 75, 79, 255);
        private static readonly PREACTColor DarkLiverHorses = new PREACTColor(84, 61, 55, 255);
        private static readonly PREACTColor DarkMagenta = new PREACTColor(139, 0, 139, 255);
        private static readonly PREACTColor DarkMidnightBlue = new PREACTColor(0, 51, 102, 255);
        private static readonly PREACTColor DarkMossGreen = new PREACTColor(74, 93, 35, 255);
        private static readonly PREACTColor DarkOliveGreen = new PREACTColor(85, 107, 47, 255);
        private static readonly PREACTColor DarkOrange = new PREACTColor(255, 140, 0, 255);
        private static readonly PREACTColor DarkOrchid = new PREACTColor(153, 50, 204, 255);
        private static readonly PREACTColor DarkPastelBlue = new PREACTColor(119, 158, 203, 255);
        private static readonly PREACTColor DarkPastelGreen = new PREACTColor(3, 192, 60, 255);
        private static readonly PREACTColor DarkPastelPurple = new PREACTColor(150, 111, 214, 255);
        private static readonly PREACTColor DarkPastelRed = new PREACTColor(194, 59, 34, 255);
        private static readonly PREACTColor DarkPink = new PREACTColor(231, 84, 128, 255);
        private static readonly PREACTColor DarkPowderBlue = new PREACTColor(0, 51, 153, 255);
        private static readonly PREACTColor DarkPuce = new PREACTColor(79, 58, 60, 255);
        private static readonly PREACTColor DarkPurple = new PREACTColor(48, 25, 52, 255);
        private static readonly PREACTColor DarkRaspberry = new PREACTColor(135, 38, 87, 255);
        private static readonly PREACTColor DarkRed = new PREACTColor(139, 0, 0, 255);
        private static readonly PREACTColor DarkSalmon = new PREACTColor(233, 150, 122, 255);
        private static readonly PREACTColor DarkScarlet = new PREACTColor(86, 3, 25, 255);
        private static readonly PREACTColor DarkSeaGreen = new PREACTColor(143, 188, 143, 255);
        private static readonly PREACTColor DarkSienna = new PREACTColor(60, 20, 20, 255);
        private static readonly PREACTColor DarkSkyBlue = new PREACTColor(140, 190, 214, 255);
        private static readonly PREACTColor DarkSlateBlue = new PREACTColor(72, 61, 139, 255);
        private static readonly PREACTColor DarkSlateGray = new PREACTColor(47, 79, 79, 255);
        private static readonly PREACTColor DarkSpringGreen = new PREACTColor(23, 114, 69, 255);
        private static readonly PREACTColor DarkTan = new PREACTColor(145, 129, 81, 255);
        private static readonly PREACTColor DarkTangerine = new PREACTColor(255, 168, 18, 255);
        private static readonly PREACTColor DarkTerraCotta = new PREACTColor(204, 78, 92, 255);
        private static readonly PREACTColor DarkTurquoise = new PREACTColor(0, 206, 209, 255);
        private static readonly PREACTColor DarkVanilla = new PREACTColor(209, 190, 168, 255);
        private static readonly PREACTColor DarkViolet = new PREACTColor(148, 0, 211, 255);
        private static readonly PREACTColor DarkYellow = new PREACTColor(155, 135, 12, 255);
        private static readonly PREACTColor DartmouthGreen = new PREACTColor(0, 112, 60, 255);
        private static readonly PREACTColor DavysGrey = new PREACTColor(85, 85, 85, 255);
        private static readonly PREACTColor DebianRed = new PREACTColor(215, 10, 83, 255);
        private static readonly PREACTColor DeepAmethyst = new PREACTColor(156, 138, 164, 255);
        private static readonly PREACTColor DeepAquamarine = new PREACTColor(64, 130, 109, 255);
        private static readonly PREACTColor DeepCarmine = new PREACTColor(169, 32, 62, 255);
        private static readonly PREACTColor DeepCarminePink = new PREACTColor(239, 48, 56, 255);
        private static readonly PREACTColor DeepCarrotOrange = new PREACTColor(233, 105, 44, 255);
        private static readonly PREACTColor DeepCerise = new PREACTColor(218, 50, 135, 255);
        private static readonly PREACTColor DeepChampagne = new PREACTColor(250, 214, 165, 255);
        private static readonly PREACTColor DeepChestnut = new PREACTColor(185, 78, 72, 255);
        private static readonly PREACTColor DeepCoffee = new PREACTColor(112, 66, 65, 255);
        private static readonly PREACTColor DeepFuchsia = new PREACTColor(193, 84, 193, 255);
        private static readonly PREACTColor DeepGreen = new PREACTColor(5, 102, 8, 255);
        private static readonly PREACTColor DeepGreenCyanTurquoise = new PREACTColor(14, 124, 97, 255);
        private static readonly PREACTColor DeepJungleGreen = new PREACTColor(0, 75, 73, 255);
        private static readonly PREACTColor DeepKoamaru = new PREACTColor(51, 51, 102, 255);
        private static readonly PREACTColor DeepLemon = new PREACTColor(245, 199, 26, 255);
        private static readonly PREACTColor DeepLilac = new PREACTColor(153, 85, 187, 255);
        private static readonly PREACTColor DeepMagenta = new PREACTColor(204, 0, 204, 255);
        private static readonly PREACTColor DeepMaroon = new PREACTColor(130, 0, 0, 255);
        private static readonly PREACTColor DeepMauve = new PREACTColor(212, 115, 212, 255);
        private static readonly PREACTColor DeepMossGreen = new PREACTColor(53, 94, 59, 255);
        private static readonly PREACTColor DeepPeach = new PREACTColor(255, 203, 164, 255);
        private static readonly PREACTColor DeepPink = new PREACTColor(255, 20, 147, 255);
        private static readonly PREACTColor DeepPuce = new PREACTColor(169, 92, 104, 255);
        private static readonly PREACTColor DeepRed = new PREACTColor(133, 1, 1, 255);
        private static readonly PREACTColor DeepRuby = new PREACTColor(132, 63, 91, 255);
        private static readonly PREACTColor DeepSaffron = new PREACTColor(255, 153, 51, 255);
        private static readonly PREACTColor DeepSpaceSparkle = new PREACTColor(74, 100, 108, 255);
        private static readonly PREACTColor DeepTaupe = new PREACTColor(126, 94, 96, 255);
        private static readonly PREACTColor DeepTuscanRed = new PREACTColor(102, 66, 77, 255);
        private static readonly PREACTColor DeepViolet = new PREACTColor(51, 0, 102, 255);
        private static readonly PREACTColor Deer = new PREACTColor(186, 135, 89, 255);
        private static readonly PREACTColor Denim = new PREACTColor(21, 96, 189, 255);
        private static readonly PREACTColor DenimBlue = new PREACTColor(34, 67, 182, 255);
        private static readonly PREACTColor DesaturatedCyan = new PREACTColor(102, 153, 153, 255);
        private static readonly PREACTColor DesertSand = new PREACTColor(237, 201, 175, 255);
        private static readonly PREACTColor Desire = new PREACTColor(234, 60, 83, 255);
        private static readonly PREACTColor Diamond = new PREACTColor(185, 242, 255, 255);
        private static readonly PREACTColor DimGray = new PREACTColor(105, 105, 105, 255);
        private static readonly PREACTColor DingyDungeon = new PREACTColor(197, 49, 81, 255);
        private static readonly PREACTColor Dirt = new PREACTColor(155, 118, 83, 255);
        private static readonly PREACTColor DirtyBrown = new PREACTColor(181, 101, 30, 255);
        private static readonly PREACTColor DirtyWhite = new PREACTColor(232, 228, 201, 255);
        private static readonly PREACTColor DodgerBlue = new PREACTColor(30, 144, 255, 255);
        private static readonly PREACTColor DodieYellow = new PREACTColor(254, 246, 91, 255);
        private static readonly PREACTColor DogwoodRose = new PREACTColor(215, 24, 104, 255);
        private static readonly PREACTColor DollarBill = new PREACTColor(133, 187, 101, 255);
        private static readonly PREACTColor DolphinGray = new PREACTColor(130, 142, 132, 255);
        private static readonly PREACTColor DonkeyBrown = new PREACTColor(102, 76, 40, 255);
        private static readonly PREACTColor DukeBlue = new PREACTColor(0, 0, 156, 255);
        private static readonly PREACTColor DustStorm = new PREACTColor(229, 204, 201, 255);
        private static readonly PREACTColor DutchWhite = new PREACTColor(239, 223, 187, 255);
        private static readonly PREACTColor EarthYellow = new PREACTColor(225, 169, 95, 255);
        private static readonly PREACTColor Ebony = new PREACTColor(85, 93, 80, 255);
        private static readonly PREACTColor Ecru = new PREACTColor(194, 178, 128, 255);
        private static readonly PREACTColor EerieBlack = new PREACTColor(27, 27, 27, 255);
        private static readonly PREACTColor Eggplant = new PREACTColor(97, 64, 81, 255);
        private static readonly PREACTColor Eggshell = new PREACTColor(240, 234, 214, 255);
        private static readonly PREACTColor EgyptianBlue = new PREACTColor(16, 52, 166, 255);
        private static readonly PREACTColor ElectricBlue = new PREACTColor(125, 249, 255, 255);
        private static readonly PREACTColor ElectricCrimson = new PREACTColor(255, 0, 63, 255);
        private static readonly PREACTColor ElectricGreen = new PREACTColor(0, 255, 0, 255);
        private static readonly PREACTColor ElectricIndigo = new PREACTColor(111, 0, 255, 255);
        private static readonly PREACTColor ElectricLime = new PREACTColor(204, 255, 0, 255);
        private static readonly PREACTColor ElectricPurple = new PREACTColor(191, 0, 255, 255);
        private static readonly PREACTColor ElectricUltramarine = new PREACTColor(63, 0, 255, 255);
        private static readonly PREACTColor ElectricViolet = new PREACTColor(143, 0, 255, 255);
        private static readonly PREACTColor ElectricYellow = new PREACTColor(255, 255, 51, 255);
        private static readonly PREACTColor Emerald = new PREACTColor(80, 200, 120, 255);
        private static readonly PREACTColor EmeraldGreen = new PREACTColor(4, 99, 7, 255);
        private static readonly PREACTColor Eminence = new PREACTColor(108, 48, 130, 255);
        private static readonly PREACTColor EnglishLavender = new PREACTColor(180, 131, 149, 255);
        private static readonly PREACTColor EnglishRed = new PREACTColor(171, 75, 82, 255);
        private static readonly PREACTColor EnglishVermillion = new PREACTColor(204, 71, 75, 255);
        private static readonly PREACTColor EnglishViolet = new PREACTColor(86, 60, 92, 255);
        private static readonly PREACTColor EtonBlue = new PREACTColor(150, 200, 162, 255);
        private static readonly PREACTColor Eucalyptus = new PREACTColor(68, 215, 168, 255);
        private static readonly PREACTColor FaluRed = new PREACTColor(128, 24, 24, 255);
        private static readonly PREACTColor Fandango = new PREACTColor(181, 51, 137, 255);
        private static readonly PREACTColor FandangoPink = new PREACTColor(222, 82, 133, 255);
        private static readonly PREACTColor FashionFuchsia = new PREACTColor(244, 0, 161, 255);
        private static readonly PREACTColor Fawn = new PREACTColor(229, 170, 112, 255);
        private static readonly PREACTColor Feldgrau = new PREACTColor(77, 93, 83, 255);
        private static readonly PREACTColor Feldspar = new PREACTColor(253, 213, 177, 255);
        private static readonly PREACTColor FernGreen = new PREACTColor(79, 121, 66, 255);
        private static readonly PREACTColor FerrariRed = new PREACTColor(255, 40, 0, 255);
        private static readonly PREACTColor FieldDrab = new PREACTColor(108, 84, 30, 255);
        private static readonly PREACTColor FieryRose = new PREACTColor(255, 84, 112, 255);
        private static readonly PREACTColor Firebrick = new PREACTColor(178, 34, 34, 255);
        private static readonly PREACTColor FireEngineRed = new PREACTColor(206, 32, 41, 255);
        private static readonly PREACTColor FireOpal = new PREACTColor(233, 92, 75, 255);
        private static readonly PREACTColor Flame = new PREACTColor(226, 88, 34, 255);
        private static readonly PREACTColor FlamingoPink = new PREACTColor(252, 142, 172, 255);
        private static readonly PREACTColor Flavescent = new PREACTColor(247, 233, 142, 255);
        private static readonly PREACTColor Flax = new PREACTColor(238, 220, 130, 255);
        private static readonly PREACTColor Flesh = new PREACTColor(255, 233, 209, 255);
        private static readonly PREACTColor Flirt = new PREACTColor(162, 0, 109, 255);
        private static readonly PREACTColor FloralWhite = new PREACTColor(255, 250, 240, 255);
        private static readonly PREACTColor Folly = new PREACTColor(255, 0, 79, 255);
        private static readonly PREACTColor ForestGreenTraditional = new PREACTColor(1, 68, 33, 255);
        private static readonly PREACTColor ForestGreenWeb = new PREACTColor(34, 139, 34, 255);
        private static readonly PREACTColor FrenchBistre = new PREACTColor(133, 109, 77, 255);
        private static readonly PREACTColor FrenchBlue = new PREACTColor(0, 114, 187, 255);
        private static readonly PREACTColor FrenchFuchsia = new PREACTColor(253, 63, 146, 255);
        private static readonly PREACTColor FrenchLilac = new PREACTColor(134, 96, 142, 255);
        private static readonly PREACTColor FrenchLime = new PREACTColor(158, 253, 56, 255);
        private static readonly PREACTColor FrenchPink = new PREACTColor(253, 108, 158, 255);
        private static readonly PREACTColor FrenchPlum = new PREACTColor(129, 20, 83, 255);
        private static readonly PREACTColor FrenchPuce = new PREACTColor(78, 22, 9, 255);
        private static readonly PREACTColor FrenchRaspberry = new PREACTColor(199, 44, 72, 255);
        private static readonly PREACTColor FrenchRose = new PREACTColor(246, 74, 138, 255);
        private static readonly PREACTColor FrenchSkyBlue = new PREACTColor(119, 181, 254, 255);
        private static readonly PREACTColor FrenchViolet = new PREACTColor(136, 6, 206, 255);
        private static readonly PREACTColor FrenchWine = new PREACTColor(172, 30, 68, 255);
        private static readonly PREACTColor FreshAir = new PREACTColor(166, 231, 255, 255);
        private static readonly PREACTColor Frostbite = new PREACTColor(233, 54, 167, 255);
        private static readonly PREACTColor Fuchsia = new PREACTColor(255, 0, 255, 255);
        private static readonly PREACTColor FuchsiaPink = new PREACTColor(255, 119, 255, 255);
        private static readonly PREACTColor FuchsiaPurple = new PREACTColor(204, 57, 123, 255);
        private static readonly PREACTColor FuchsiaRose = new PREACTColor(199, 67, 117, 255);
        private static readonly PREACTColor Fulvous = new PREACTColor(228, 132, 0, 255);
        private static readonly PREACTColor FuzzyWuzzy = new PREACTColor(204, 102, 102, 255);
        private static readonly PREACTColor Gainsboro = new PREACTColor(220, 220, 220, 255);
        private static readonly PREACTColor Gamboge = new PREACTColor(228, 155, 15, 255);
        private static readonly PREACTColor GambogeOrangeBrown = new PREACTColor(152, 102, 0, 255);
        private static readonly PREACTColor Garnet = new PREACTColor(115, 54, 53, 255);
        private static readonly PREACTColor GargoyleGas = new PREACTColor(255, 223, 70, 255);
        private static readonly PREACTColor GenericViridian = new PREACTColor(0, 127, 102, 255);
        private static readonly PREACTColor GhostWhite = new PREACTColor(248, 248, 255, 255);
        private static readonly PREACTColor GiantsClub = new PREACTColor(176, 92, 82, 255);
        private static readonly PREACTColor GiantsOrange = new PREACTColor(254, 90, 29, 255);
        private static readonly PREACTColor Glaucous = new PREACTColor(96, 130, 182, 255);
        private static readonly PREACTColor GlossyGrape = new PREACTColor(171, 146, 179, 255);
        private static readonly PREACTColor GOGreen = new PREACTColor(0, 171, 102, 255);
        private static readonly PREACTColor Gold = new PREACTColor(165, 124, 0, 255);
        private static readonly PREACTColor GoldMetallic = new PREACTColor(212, 175, 55, 255);
        private static readonly PREACTColor GoldWebGolden = new PREACTColor(255, 215, 0, 255);
        private static readonly PREACTColor GoldCrayola = new PREACTColor(230, 190, 138, 255);
        private static readonly PREACTColor GoldFusion = new PREACTColor(133, 117, 78, 255);
        private static readonly PREACTColor GoldFoil = new PREACTColor(189, 155, 22, 255);
        private static readonly PREACTColor GoldenBrown = new PREACTColor(153, 101, 21, 255);
        private static readonly PREACTColor GoldenPoppy = new PREACTColor(252, 194, 0, 255);
        private static readonly PREACTColor GoldenYellow = new PREACTColor(255, 223, 0, 255);
        private static readonly PREACTColor Goldenrod = new PREACTColor(218, 165, 32, 255);
        private static readonly PREACTColor GraniteGray = new PREACTColor(103, 103, 103, 255);
        private static readonly PREACTColor GrannySmithApple = new PREACTColor(168, 228, 160, 255);
        private static readonly PREACTColor Grape = new PREACTColor(111, 45, 168, 255);
        private static readonly PREACTColor GrayHTMLCSSGray = new PREACTColor(128, 128, 128, 255);
        private static readonly PREACTColor GrayX11Gray = new PREACTColor(190, 190, 190, 255);
        private static readonly PREACTColor GrayAsparagus = new PREACTColor(70, 89, 69, 255);
        private static readonly PREACTColor Green = new PREACTColor(0, 128, 1, 255);
        private static readonly PREACTColor GreenCrayola = new PREACTColor(28, 172, 120, 255);
        private static readonly PREACTColor GreenMunsell = new PREACTColor(0, 168, 119, 255);
        private static readonly PREACTColor GreenNCS = new PREACTColor(0, 159, 107, 255);
        private static readonly PREACTColor GreenPantone = new PREACTColor(0, 173, 67, 255);
        private static readonly PREACTColor GreenPigment = new PREACTColor(0, 165, 80, 255);
        private static readonly PREACTColor GreenRYB = new PREACTColor(102, 176, 50, 255);
        private static readonly PREACTColor GreenBlue = new PREACTColor(17, 100, 180, 255);
        private static readonly PREACTColor GreenCyan = new PREACTColor(0, 153, 102, 255);
        private static readonly PREACTColor GreenLizard = new PREACTColor(167, 244, 50, 255);
        private static readonly PREACTColor GreenSheen = new PREACTColor(110, 174, 161, 255);
        private static readonly PREACTColor GreenYellow = new PREACTColor(173, 255, 47, 255);
        private static readonly PREACTColor Grullo = new PREACTColor(169, 154, 134, 255);
        private static readonly PREACTColor GuppieGreen = new PREACTColor(0, 255, 127, 255);
        private static readonly PREACTColor Gunmetal = new PREACTColor(42, 52, 57, 255);
        private static readonly PREACTColor HalayàÚbe = new PREACTColor(102, 55, 84, 255);
        private static readonly PREACTColor HalloweenOrange = new PREACTColor(235, 97, 35, 255);
        private static readonly PREACTColor HanBlue = new PREACTColor(68, 108, 207, 255);
        private static readonly PREACTColor HanPurple = new PREACTColor(82, 24, 250, 255);
        private static readonly PREACTColor Harlequin = new PREACTColor(63, 255, 0, 255);
        private static readonly PREACTColor HarlequinGreen = new PREACTColor(70, 203, 24, 255);
        private static readonly PREACTColor HarvardCrimson = new PREACTColor(201, 0, 22, 255);
        private static readonly PREACTColor HarvestGold = new PREACTColor(218, 145, 0, 255);
        private static readonly PREACTColor HeartGold = new PREACTColor(128, 128, 0, 255);
        private static readonly PREACTColor HeatWave = new PREACTColor(255, 122, 0, 255);
        private static readonly PREACTColor Heliotrope = new PREACTColor(223, 115, 255, 255);
        private static readonly PREACTColor HeliotropeGray = new PREACTColor(170, 152, 168, 255);
        private static readonly PREACTColor HeliotropeMagenta = new PREACTColor(170, 0, 187, 255);
        private static readonly PREACTColor Honeydew = new PREACTColor(240, 255, 240, 255);
        private static readonly PREACTColor HonoluluBlue = new PREACTColor(0, 109, 176, 255);
        private static readonly PREACTColor HookersGreen = new PREACTColor(73, 121, 107, 255);
        private static readonly PREACTColor HotMagenta = new PREACTColor(255, 29, 206, 255);
        private static readonly PREACTColor HotPink = new PREACTColor(255, 105, 180, 255);
        private static readonly PREACTColor Iceberg = new PREACTColor(113, 166, 210, 255);
        private static readonly PREACTColor Icterine = new PREACTColor(252, 247, 94, 255);
        private static readonly PREACTColor IguanaGreen = new PREACTColor(113, 188, 120, 255);
        private static readonly PREACTColor IlluminatingEmerald = new PREACTColor(49, 145, 119, 255);
        private static readonly PREACTColor Imperial = new PREACTColor(96, 47, 107, 255);
        private static readonly PREACTColor ImperialBlue = new PREACTColor(0, 35, 149, 255);
        private static readonly PREACTColor ImperialPurple = new PREACTColor(102, 2, 60, 255);
        private static readonly PREACTColor ImperialRed = new PREACTColor(237, 41, 57, 255);
        private static readonly PREACTColor Inchworm = new PREACTColor(178, 236, 93, 255);
        private static readonly PREACTColor Independence = new PREACTColor(76, 81, 109, 255);
        private static readonly PREACTColor IndiaGreen = new PREACTColor(19, 136, 8, 255);
        private static readonly PREACTColor IndianRed = new PREACTColor(205, 92, 92, 255);
        private static readonly PREACTColor IndianYellow = new PREACTColor(227, 168, 87, 255);
        private static readonly PREACTColor Indigo = new PREACTColor(75, 0, 130, 255);
        private static readonly PREACTColor IndigoDye = new PREACTColor(9, 31, 146, 255);
        private static readonly PREACTColor IndigoRainbow = new PREACTColor(35, 48, 103, 255);
        private static readonly PREACTColor InfraRed = new PREACTColor(255, 73, 108, 255);
        private static readonly PREACTColor InterdimensionalBlue = new PREACTColor(54, 12, 204, 255);
        private static readonly PREACTColor InternationalKleinBlue = new PREACTColor(0, 47, 167, 255);
        private static readonly PREACTColor InternationalOrangeAerospace = new PREACTColor(255, 79, 0, 255);
        private static readonly PREACTColor InternationalOrangeEngineering = new PREACTColor(186, 22, 12, 255);
        private static readonly PREACTColor InternationalOrangeGoldenGateBridge = new PREACTColor(192, 54, 44, 255);
        private static readonly PREACTColor Iris = new PREACTColor(90, 79, 207, 255);
        private static readonly PREACTColor Irresistible = new PREACTColor(179, 68, 108, 255);
        private static readonly PREACTColor Isabelline = new PREACTColor(244, 240, 236, 255);
        private static readonly PREACTColor IslamicGreen = new PREACTColor(0, 144, 0, 255);
        private static readonly PREACTColor Ivory = new PREACTColor(255, 255, 240, 255);
        private static readonly PREACTColor Jacarta = new PREACTColor(61, 50, 93, 255);
        private static readonly PREACTColor JackoBean = new PREACTColor(65, 54, 40, 255);
        private static readonly PREACTColor Jade = new PREACTColor(0, 168, 107, 255);
        private static readonly PREACTColor JapaneseCarmine = new PREACTColor(157, 41, 51, 255);
        private static readonly PREACTColor JapaneseIndigo = new PREACTColor(38, 67, 72, 255);
        private static readonly PREACTColor JapaneseLaurel = new PREACTColor(47, 117, 50, 255);
        private static readonly PREACTColor JapaneseViolet = new PREACTColor(91, 50, 86, 255);
        private static readonly PREACTColor Jasmine = new PREACTColor(248, 222, 126, 255);
        private static readonly PREACTColor Jasper = new PREACTColor(215, 59, 62, 255);
        private static readonly PREACTColor JasperOrange = new PREACTColor(223, 145, 79, 255);
        private static readonly PREACTColor JazzberryJam = new PREACTColor(165, 11, 94, 255);
        private static readonly PREACTColor JellyBean = new PREACTColor(218, 97, 78, 255);
        private static readonly PREACTColor JellyBeanBlue = new PREACTColor(68, 121, 142, 255);
        private static readonly PREACTColor Jet = new PREACTColor(52, 52, 52, 255);
        private static readonly PREACTColor JetStream = new PREACTColor(187, 208, 201, 255);
        private static readonly PREACTColor Jonquil = new PREACTColor(244, 202, 22, 255);
        private static readonly PREACTColor JordyBlue = new PREACTColor(138, 185, 241, 255);
        private static readonly PREACTColor JuneBud = new PREACTColor(189, 218, 87, 255);
        private static readonly PREACTColor JungleGreen = new PREACTColor(41, 171, 135, 255);
        private static readonly PREACTColor KellyGreen = new PREACTColor(76, 187, 23, 255);
        private static readonly PREACTColor KenyanCopper = new PREACTColor(124, 28, 5, 255);
        private static readonly PREACTColor Keppel = new PREACTColor(58, 176, 158, 255);
        private static readonly PREACTColor KeyLime = new PREACTColor(232, 244, 140, 255);
        private static readonly PREACTColor KhakiHTMLCSSKhaki = new PREACTColor(195, 176, 145, 255);
        private static readonly PREACTColor KhakiX11LightKhaki = new PREACTColor(240, 230, 140, 255);
        private static readonly PREACTColor Kiwi = new PREACTColor(142, 229, 63, 255);
        private static readonly PREACTColor Kobe = new PREACTColor(136, 45, 23, 255);
        private static readonly PREACTColor Kobi = new PREACTColor(231, 159, 196, 255);
        private static readonly PREACTColor KombuGreen = new PREACTColor(53, 66, 48, 255);
        private static readonly PREACTColor KSUPurple = new PREACTColor(79, 38, 131, 255);
        private static readonly PREACTColor KUCrimson = new PREACTColor(232, 0, 13, 255);
        private static readonly PREACTColor LaSalleGreen = new PREACTColor(8, 120, 48, 255);
        private static readonly PREACTColor LanguidLavender = new PREACTColor(214, 202, 221, 255);
        private static readonly PREACTColor LapisLazuli = new PREACTColor(38, 97, 156, 255);
        private static readonly PREACTColor LaserLemon = new PREACTColor(255, 255, 102, 255);
        private static readonly PREACTColor LaurelGreen = new PREACTColor(169, 186, 157, 255);
        private static readonly PREACTColor Lava = new PREACTColor(207, 16, 32, 255);
        private static readonly PREACTColor LavenderFloral = new PREACTColor(181, 126, 220, 255);
        private static readonly PREACTColor LavenderWeb = new PREACTColor(230, 230, 250, 255);
        private static readonly PREACTColor LavenderBlue = new PREACTColor(204, 204, 255, 255);
        private static readonly PREACTColor LavenderBlush = new PREACTColor(255, 240, 245, 255);
        private static readonly PREACTColor LavenderGray = new PREACTColor(196, 195, 208, 255);
        private static readonly PREACTColor LavenderIndigo = new PREACTColor(148, 87, 235, 255);
        private static readonly PREACTColor LavenderMagenta = new PREACTColor(238, 130, 238, 255);
        private static readonly PREACTColor LavenderPink = new PREACTColor(251, 174, 210, 255);
        private static readonly PREACTColor LavenderPurple = new PREACTColor(150, 123, 182, 255);
        private static readonly PREACTColor LavenderRose = new PREACTColor(251, 160, 227, 255);
        private static readonly PREACTColor LawnGreen = new PREACTColor(124, 252, 0, 255);
        private static readonly PREACTColor Lemon = new PREACTColor(255, 247, 0, 255);
        private static readonly PREACTColor LemonChiffon = new PREACTColor(255, 250, 205, 255);
        private static readonly PREACTColor LemonCurry = new PREACTColor(204, 160, 29, 255);
        private static readonly PREACTColor LemonGlacier = new PREACTColor(253, 255, 0, 255);
        private static readonly PREACTColor LemonMeringue = new PREACTColor(246, 234, 190, 255);
        private static readonly PREACTColor LemonYellow = new PREACTColor(255, 244, 79, 255);
        private static readonly PREACTColor LemonYellowCrayola = new PREACTColor(255, 255, 159, 255);
        private static readonly PREACTColor Lenurple = new PREACTColor(186, 147, 216, 255);
        private static readonly PREACTColor Liberty = new PREACTColor(84, 90, 167, 255);
        private static readonly PREACTColor Licorice = new PREACTColor(26, 17, 16, 255);
        private static readonly PREACTColor LightBlue = new PREACTColor(173, 216, 230, 255);
        private static readonly PREACTColor LightBrown = new PREACTColor(181, 101, 29, 255);
        private static readonly PREACTColor LightCarminePink = new PREACTColor(230, 103, 113, 255);
        private static readonly PREACTColor LightCobaltBlue = new PREACTColor(136, 172, 224, 255);
        private static readonly PREACTColor LightCoral = new PREACTColor(240, 128, 128, 255);
        private static readonly PREACTColor LightCornflowerBlue = new PREACTColor(147, 204, 234, 255);
        private static readonly PREACTColor LightCrimson = new PREACTColor(245, 105, 145, 255);
        private static readonly PREACTColor LightCyan = new PREACTColor(224, 255, 255, 255);
        private static readonly PREACTColor LightDeepPink = new PREACTColor(255, 92, 205, 255);
        private static readonly PREACTColor LightFrenchBeige = new PREACTColor(200, 173, 127, 255);
        private static readonly PREACTColor LightFuchsiaPink = new PREACTColor(249, 132, 239, 255);
        private static readonly PREACTColor LightGold = new PREACTColor(178, 151, 0, 255);
        private static readonly PREACTColor LightGoldenrodYellow = new PREACTColor(250, 250, 210, 255);
        private static readonly PREACTColor LightGray = new PREACTColor(211, 211, 211, 255);
        private static readonly PREACTColor LightGrayishMagenta = new PREACTColor(204, 153, 204, 255);
        private static readonly PREACTColor LightGreen = new PREACTColor(144, 238, 144, 255);
        private static readonly PREACTColor LightHotPink = new PREACTColor(255, 179, 222, 255);
        private static readonly PREACTColor LightMediumOrchid = new PREACTColor(211, 155, 203, 255);
        private static readonly PREACTColor LightMossGreen = new PREACTColor(173, 223, 173, 255);
        private static readonly PREACTColor LightOrange = new PREACTColor(254, 216, 177, 255);
        private static readonly PREACTColor LightOrchid = new PREACTColor(230, 168, 215, 255);
        private static readonly PREACTColor LightPastelPurple = new PREACTColor(177, 156, 217, 255);
        private static readonly PREACTColor LightPeriwinkle = new PREACTColor(197, 203, 225, 255);
        private static readonly PREACTColor LightPink = new PREACTColor(255, 182, 193, 255);
        private static readonly PREACTColor LightSalmon = new PREACTColor(255, 160, 122, 255);
        private static readonly PREACTColor LightSalmonPink = new PREACTColor(255, 153, 153, 255);
        private static readonly PREACTColor LightSeaGreen = new PREACTColor(32, 178, 170, 255);
        private static readonly PREACTColor LightSilver = new PREACTColor(216, 216, 216, 255);
        private static readonly PREACTColor LightSkyBlue = new PREACTColor(135, 206, 250, 255);
        private static readonly PREACTColor LightSlateGray = new PREACTColor(119, 136, 153, 255);
        private static readonly PREACTColor LightSteelBlue = new PREACTColor(176, 196, 222, 255);
        private static readonly PREACTColor LightTaupe = new PREACTColor(179, 139, 109, 255);
        private static readonly PREACTColor LightYellow = new PREACTColor(255, 255, 224, 255);
        private static readonly PREACTColor Lilac = new PREACTColor(200, 162, 200, 255);
        private static readonly PREACTColor LilacLuster = new PREACTColor(174, 152, 170, 255);
        private static readonly PREACTColor LimeGreen = new PREACTColor(50, 205, 50, 255);
        private static readonly PREACTColor Limerick = new PREACTColor(157, 194, 9, 255);
        private static readonly PREACTColor LincolnGreen = new PREACTColor(25, 89, 5, 255);
        private static readonly PREACTColor Linen = new PREACTColor(250, 240, 230, 255);
        private static readonly PREACTColor LittleBoyBlue = new PREACTColor(108, 160, 220, 255);
        private static readonly PREACTColor LittleGirlPink = new PREACTColor(248, 185, 212, 255);
        private static readonly PREACTColor Liver = new PREACTColor(103, 76, 71, 255);
        private static readonly PREACTColor LiverDogs = new PREACTColor(184, 109, 41, 255);
        private static readonly PREACTColor LiverOrgan = new PREACTColor(108, 46, 31, 255);
        private static readonly PREACTColor LiverChestnut = new PREACTColor(152, 116, 86, 255);
        private static readonly PREACTColor Lotion = new PREACTColor(255, 254, 250, 255);
        private static readonly PREACTColor Lumber = new PREACTColor(255, 228, 205, 255);
        private static readonly PREACTColor Lust = new PREACTColor(230, 32, 32, 255);
        private static readonly PREACTColor MaastrichtBlue = new PREACTColor(0, 28, 61, 255);
        private static readonly PREACTColor MacaroniAndCheese = new PREACTColor(255, 189, 136, 255);
        private static readonly PREACTColor MadderLake = new PREACTColor(204, 51, 54, 255);
        private static readonly PREACTColor MagentaDye = new PREACTColor(202, 31, 123, 255);
        private static readonly PREACTColor MagentaPantone = new PREACTColor(208, 65, 126, 255);
        private static readonly PREACTColor MagentaProcess = new PREACTColor(255, 0, 144, 255);
        private static readonly PREACTColor MagentaHaze = new PREACTColor(159, 69, 118, 255);
        private static readonly PREACTColor MagentaPink = new PREACTColor(204, 51, 139, 255);
        private static readonly PREACTColor MagicMint = new PREACTColor(170, 240, 209, 255);
        private static readonly PREACTColor MagicPotion = new PREACTColor(255, 68, 102, 255);
        private static readonly PREACTColor Magnolia = new PREACTColor(248, 244, 255, 255);
        private static readonly PREACTColor Mahogany = new PREACTColor(192, 64, 0, 255);
        private static readonly PREACTColor MaizeCrayola = new PREACTColor(242, 198, 73, 255);
        private static readonly PREACTColor MajorelleBlue = new PREACTColor(96, 80, 220, 255);
        private static readonly PREACTColor Malachite = new PREACTColor(11, 218, 81, 255);
        private static readonly PREACTColor Manatee = new PREACTColor(151, 154, 170, 255);
        private static readonly PREACTColor Mandarin = new PREACTColor(243, 122, 72, 255);
        private static readonly PREACTColor MangoGreen = new PREACTColor(150, 255, 0, 255);
        private static readonly PREACTColor MangoTango = new PREACTColor(255, 130, 67, 255);
        private static readonly PREACTColor Mantis = new PREACTColor(116, 195, 101, 255);
        private static readonly PREACTColor MardiGras = new PREACTColor(136, 0, 133, 255);
        private static readonly PREACTColor Marigold = new PREACTColor(234, 162, 33, 255);
        private static readonly PREACTColor MaroonHTMLCSS = new PREACTColor(128, 0, 0, 255);
        private static readonly PREACTColor MaroonX11 = new PREACTColor(176, 48, 96, 255);
        private static readonly PREACTColor Mauve = new PREACTColor(224, 176, 255, 255);
        private static readonly PREACTColor MauveTaupe = new PREACTColor(145, 95, 109, 255);
        private static readonly PREACTColor Mauvelous = new PREACTColor(239, 152, 170, 255);
        private static readonly PREACTColor MaximumBlue = new PREACTColor(71, 171, 204, 255);
        private static readonly PREACTColor MaximumBlueGreen = new PREACTColor(48, 191, 191, 255);
        private static readonly PREACTColor MaximumBluePurple = new PREACTColor(172, 172, 230, 255);
        private static readonly PREACTColor MaximumGreen = new PREACTColor(94, 140, 49, 255);
        private static readonly PREACTColor MaximumGreenYellow = new PREACTColor(217, 230, 80, 255);
        private static readonly PREACTColor MaximumPurple = new PREACTColor(115, 51, 128, 255);
        private static readonly PREACTColor MaximumRed = new PREACTColor(217, 33, 33, 255);
        private static readonly PREACTColor MaximumRedPurple = new PREACTColor(166, 58, 121, 255);
        private static readonly PREACTColor MaximumYellow = new PREACTColor(250, 250, 55, 255);
        private static readonly PREACTColor MaximumYellowRed = new PREACTColor(242, 186, 73, 255);
        private static readonly PREACTColor MayGreen = new PREACTColor(76, 145, 65, 255);
        private static readonly PREACTColor MayaBlue = new PREACTColor(115, 194, 251, 255);
        private static readonly PREACTColor MeatBrown = new PREACTColor(229, 183, 59, 255);
        private static readonly PREACTColor MediumAquamarine = new PREACTColor(102, 221, 170, 255);
        private static readonly PREACTColor MediumBlue = new PREACTColor(0, 0, 205, 255);
        private static readonly PREACTColor MediumCandyAppleRed = new PREACTColor(226, 6, 44, 255);
        private static readonly PREACTColor MediumCarmine = new PREACTColor(175, 64, 53, 255);
        private static readonly PREACTColor MediumChampagne = new PREACTColor(243, 229, 171, 255);
        private static readonly PREACTColor MediumElectricBlue = new PREACTColor(3, 80, 150, 255);
        private static readonly PREACTColor MediumJungleGreen = new PREACTColor(28, 53, 45, 255);
        private static readonly PREACTColor MediumLavenderMagenta = new PREACTColor(221, 160, 221, 255);
        private static readonly PREACTColor MediumOrchid = new PREACTColor(186, 85, 211, 255);
        private static readonly PREACTColor MediumPersianBlue = new PREACTColor(0, 103, 165, 255);
        private static readonly PREACTColor MediumPurple = new PREACTColor(147, 112, 219, 255);
        private static readonly PREACTColor MediumRedViolet = new PREACTColor(187, 51, 133, 255);
        private static readonly PREACTColor MediumRuby = new PREACTColor(170, 64, 105, 255);
        private static readonly PREACTColor MediumSeaGreen = new PREACTColor(60, 179, 113, 255);
        private static readonly PREACTColor MediumSkyBlue = new PREACTColor(128, 218, 235, 255);
        private static readonly PREACTColor MediumSlateBlue = new PREACTColor(123, 104, 238, 255);
        private static readonly PREACTColor MediumSpringBud = new PREACTColor(201, 220, 135, 255);
        private static readonly PREACTColor MediumSpringGreen = new PREACTColor(0, 250, 154, 255);
        private static readonly PREACTColor MediumTurquoise = new PREACTColor(72, 209, 204, 255);
        private static readonly PREACTColor MediumVermilion = new PREACTColor(217, 96, 59, 255);
        private static readonly PREACTColor MediumVioletRed = new PREACTColor(199, 21, 133, 255);
        private static readonly PREACTColor MellowApricot = new PREACTColor(248, 184, 120, 255);
        private static readonly PREACTColor Melon = new PREACTColor(253, 188, 180, 255);
        private static readonly PREACTColor Menthol = new PREACTColor(193, 249, 162, 255);
        private static readonly PREACTColor MetallicBlue = new PREACTColor(50, 82, 123, 255);
        private static readonly PREACTColor MetallicBronze = new PREACTColor(169, 113, 66, 255);
        private static readonly PREACTColor MetallicBrown = new PREACTColor(172, 67, 19, 255);
        private static readonly PREACTColor MetallicGreen = new PREACTColor(41, 110, 1, 255);
        private static readonly PREACTColor MetallicOrange = new PREACTColor(218, 104, 15, 255);
        private static readonly PREACTColor MetallicPink = new PREACTColor(237, 166, 196, 255);
        private static readonly PREACTColor MetallicRed = new PREACTColor(166, 44, 43, 255);
        private static readonly PREACTColor MetallicSeaweed = new PREACTColor(10, 126, 140, 255);
        private static readonly PREACTColor MetallicSilver = new PREACTColor(168, 169, 173, 255);
        private static readonly PREACTColor MetallicSunburst = new PREACTColor(156, 124, 56, 255);
        private static readonly PREACTColor MetallicViolet = new PREACTColor(90, 10, 145, 255);
        private static readonly PREACTColor MetallicYellow = new PREACTColor(253, 204, 13, 255);
        private static readonly PREACTColor MexicanPink = new PREACTColor(228, 0, 124, 255);
        private static readonly PREACTColor MiddleBlue = new PREACTColor(126, 212, 230, 255);
        private static readonly PREACTColor MiddleBlueGreen = new PREACTColor(141, 217, 204, 255);
        private static readonly PREACTColor MiddleBluePurple = new PREACTColor(139, 114, 190, 255);
        private static readonly PREACTColor MiddleGrey = new PREACTColor(139, 134, 128, 255);
        private static readonly PREACTColor MiddleGreen = new PREACTColor(77, 140, 87, 255);
        private static readonly PREACTColor MiddleGreenYellow = new PREACTColor(172, 191, 96, 255);
        private static readonly PREACTColor MiddlePurple = new PREACTColor(217, 130, 181, 255);
        private static readonly PREACTColor MiddleRed = new PREACTColor(229, 144, 115, 255);
        private static readonly PREACTColor MiddleRedPurple = new PREACTColor(165, 83, 83, 255);
        private static readonly PREACTColor MiddleYellow = new PREACTColor(255, 235, 0, 255);
        private static readonly PREACTColor MiddleYellowRed = new PREACTColor(236, 177, 118, 255);
        private static readonly PREACTColor Midnight = new PREACTColor(112, 38, 112, 255);
        private static readonly PREACTColor MidnightBlue = new PREACTColor(25, 25, 112, 255);
        private static readonly PREACTColor MidnightBlue2 = new PREACTColor(0, 70, 140, 255);
        private static readonly PREACTColor MidnightGreenEagleGreen = new PREACTColor(0, 73, 83, 255);
        private static readonly PREACTColor MikadoYellow = new PREACTColor(255, 196, 12, 255);
        private static readonly PREACTColor Milk = new PREACTColor(253, 255, 245, 255);
        private static readonly PREACTColor MilkChocolate = new PREACTColor(132, 86, 60, 255);
        private static readonly PREACTColor MimiPink = new PREACTColor(255, 218, 233, 255);
        private static readonly PREACTColor Mindaro = new PREACTColor(227, 249, 136, 255);
        private static readonly PREACTColor Ming = new PREACTColor(54, 116, 125, 255);
        private static readonly PREACTColor MinionYellow = new PREACTColor(245, 220, 80, 255);
        private static readonly PREACTColor Mint = new PREACTColor(62, 180, 137, 255);
        private static readonly PREACTColor MintCream = new PREACTColor(245, 255, 250, 255);
        private static readonly PREACTColor MintGreen = new PREACTColor(152, 255, 152, 255);
        private static readonly PREACTColor MistyMoss = new PREACTColor(187, 180, 119, 255);
        private static readonly PREACTColor MistyRose = new PREACTColor(255, 228, 225, 255);
        private static readonly PREACTColor Moonstone = new PREACTColor(58, 168, 193, 255);
        private static readonly PREACTColor MoonstoneBlue = new PREACTColor(115, 169, 194, 255);
        private static readonly PREACTColor MordantRed19 = new PREACTColor(174, 12, 0, 255);
        private static readonly PREACTColor MorningBlue = new PREACTColor(141, 163, 153, 255);
        private static readonly PREACTColor MossGreen = new PREACTColor(138, 154, 91, 255);
        private static readonly PREACTColor MountainMeadow = new PREACTColor(48, 186, 143, 255);
        private static readonly PREACTColor MountbattenPink = new PREACTColor(153, 122, 141, 255);
        private static readonly PREACTColor MSUGreen = new PREACTColor(24, 69, 59, 255);
        private static readonly PREACTColor Mud = new PREACTColor(111, 83, 61, 255);
        private static readonly PREACTColor MughalGreen = new PREACTColor(48, 96, 48, 255);
        private static readonly PREACTColor Mulberry = new PREACTColor(197, 75, 140, 255);
        private static readonly PREACTColor MulberryCrayola = new PREACTColor(200, 80, 155, 255);
        private static readonly PREACTColor Mustard = new PREACTColor(255, 219, 88, 255);
        private static readonly PREACTColor MustardBrown = new PREACTColor(205, 122, 0, 255);
        private static readonly PREACTColor MustardGreen = new PREACTColor(110, 110, 48, 255);
        private static readonly PREACTColor MustardYellow = new PREACTColor(255, 173, 1, 255);
        private static readonly PREACTColor MyrtleGreen = new PREACTColor(49, 120, 115, 255);
        private static readonly PREACTColor Mystic = new PREACTColor(214, 82, 130, 255);
        private static readonly PREACTColor MysticMaroon = new PREACTColor(173, 67, 121, 255);
        private static readonly PREACTColor MysticRed = new PREACTColor(255, 34, 0, 255);
        private static readonly PREACTColor NadeshikoPink = new PREACTColor(246, 173, 198, 255);
        private static readonly PREACTColor NapierGreen = new PREACTColor(42, 128, 0, 255);
        private static readonly PREACTColor NaplesYellow = new PREACTColor(250, 218, 94, 255);
        private static readonly PREACTColor NavajoWhite = new PREACTColor(255, 222, 173, 255);
        private static readonly PREACTColor Navy = new PREACTColor(0, 0, 128, 255);
        private static readonly PREACTColor NeonBlue = new PREACTColor(27, 3, 163, 255);
        private static readonly PREACTColor NeonBrown = new PREACTColor(195, 115, 42, 255);
        private static readonly PREACTColor NeonCarrot = new PREACTColor(255, 163, 67, 255);
        private static readonly PREACTColor NeonCyan = new PREACTColor(0, 254, 252, 255);
        private static readonly PREACTColor NeonFuchsia = new PREACTColor(254, 65, 100, 255);
        private static readonly PREACTColor NeonGold = new PREACTColor(207, 170, 1, 255);
        private static readonly PREACTColor NeonGreen = new PREACTColor(57, 255, 20, 255);
        private static readonly PREACTColor NeonPink = new PREACTColor(254, 52, 126, 255);
        private static readonly PREACTColor NeonRed = new PREACTColor(255, 24, 24, 255);
        private static readonly PREACTColor NeonScarlet = new PREACTColor(255, 38, 3, 255);
        private static readonly PREACTColor NeonTangerine = new PREACTColor(246, 137, 10, 255);
        private static readonly PREACTColor NewCar = new PREACTColor(33, 79, 198, 255);
        private static readonly PREACTColor NewYorkPink = new PREACTColor(215, 131, 127, 255);
        private static readonly PREACTColor Nickel = new PREACTColor(114, 116, 114, 255);
        private static readonly PREACTColor NonPhotoBlue = new PREACTColor(164, 221, 237, 255);
        private static readonly PREACTColor NorthTexasGreen = new PREACTColor(5, 144, 51, 255);
        private static readonly PREACTColor Nyanza = new PREACTColor(233, 255, 219, 255);
        private static readonly PREACTColor OceanBlue = new PREACTColor(79, 66, 181, 255);
        private static readonly PREACTColor OceanBoatBlue = new PREACTColor(0, 119, 190, 255);
        private static readonly PREACTColor OceanGreen = new PREACTColor(72, 191, 145, 255);
        private static readonly PREACTColor Ochre = new PREACTColor(204, 119, 34, 255);
        private static readonly PREACTColor OgreOdor = new PREACTColor(253, 82, 64, 255);
        private static readonly PREACTColor OldBurgundy = new PREACTColor(67, 48, 46, 255);
        private static readonly PREACTColor OldGold = new PREACTColor(207, 181, 59, 255);
        private static readonly PREACTColor OldLace = new PREACTColor(253, 245, 230, 255);
        private static readonly PREACTColor OldLavender = new PREACTColor(121, 104, 120, 255);
        private static readonly PREACTColor OldMauve = new PREACTColor(103, 49, 71, 255);
        private static readonly PREACTColor OldMossGreen = new PREACTColor(134, 126, 54, 255);
        private static readonly PREACTColor OldRose = new PREACTColor(192, 128, 129, 255);
        private static readonly PREACTColor OliveDrab3 = new PREACTColor(107, 142, 35, 255);
        private static readonly PREACTColor OliveDrab7 = new PREACTColor(60, 52, 31, 255);
        private static readonly PREACTColor Olivine = new PREACTColor(154, 185, 115, 255);
        private static readonly PREACTColor Onyx = new PREACTColor(53, 56, 57, 255);
        private static readonly PREACTColor Opal = new PREACTColor(168, 195, 188, 255);
        private static readonly PREACTColor OperaMauve = new PREACTColor(183, 132, 167, 255);
        private static readonly PREACTColor OrangeColorWheel = new PREACTColor(255, 127, 0, 255);
        private static readonly PREACTColor OrangeCrayola = new PREACTColor(255, 117, 56, 255);
        private static readonly PREACTColor OrangePantone = new PREACTColor(255, 88, 0, 255);
        private static readonly PREACTColor OrangeRYB = new PREACTColor(251, 153, 2, 255);
        private static readonly PREACTColor OrangeWeb = new PREACTColor(255, 165, 0, 255);
        private static readonly PREACTColor OrangePeel = new PREACTColor(255, 159, 0, 255);
        private static readonly PREACTColor OrangeRed = new PREACTColor(255, 69, 0, 255);
        private static readonly PREACTColor OrangeSoda = new PREACTColor(250, 91, 61, 255);
        private static readonly PREACTColor OrangeYellow = new PREACTColor(248, 213, 104, 255);
        private static readonly PREACTColor Orchid = new PREACTColor(218, 112, 214, 255);
        private static readonly PREACTColor OrchidPink = new PREACTColor(242, 189, 205, 255);
        private static readonly PREACTColor OriolesOrange = new PREACTColor(251, 79, 20, 255);
        private static readonly PREACTColor OuterSpace = new PREACTColor(65, 74, 76, 255);
        private static readonly PREACTColor OutrageousOrange = new PREACTColor(255, 110, 74, 255);
        private static readonly PREACTColor OxfordBlue = new PREACTColor(0, 33, 71, 255);
        private static readonly PREACTColor Oxley = new PREACTColor(109, 154, 121, 255);
        private static readonly PREACTColor PacificBlue = new PREACTColor(28, 169, 201, 255);
        private static readonly PREACTColor PakistanGreen = new PREACTColor(0, 102, 0, 255);
        private static readonly PREACTColor PalatinateBlue = new PREACTColor(39, 59, 226, 255);
        private static readonly PREACTColor PalatinatePurple = new PREACTColor(104, 40, 96, 255);
        private static readonly PREACTColor PaleBlue = new PREACTColor(175, 238, 238, 255);
        private static readonly PREACTColor PaleBrown = new PREACTColor(152, 118, 84, 255);
        private static readonly PREACTColor PaleCerulean = new PREACTColor(155, 196, 226, 255);
        private static readonly PREACTColor PaleChestnut = new PREACTColor(221, 173, 175, 255);
        private static readonly PREACTColor PaleCornflowerBlue = new PREACTColor(171, 205, 239, 255);
        private static readonly PREACTColor PaleCyan = new PREACTColor(135, 211, 248, 255);
        private static readonly PREACTColor PaleGoldenrod = new PREACTColor(238, 232, 170, 255);
        private static readonly PREACTColor PaleGreen = new PREACTColor(152, 251, 152, 255);
        private static readonly PREACTColor PaleLavender = new PREACTColor(220, 208, 255, 255);
        private static readonly PREACTColor PaleMagenta = new PREACTColor(249, 132, 229, 255);
        private static readonly PREACTColor PaleMagentaPink = new PREACTColor(255, 153, 204, 255);
        private static readonly PREACTColor PalePink = new PREACTColor(250, 218, 221, 255);
        private static readonly PREACTColor PaleRedViolet = new PREACTColor(219, 112, 147, 255);
        private static readonly PREACTColor PaleRobinEggBlue = new PREACTColor(150, 222, 209, 255);
        private static readonly PREACTColor PaleSilver = new PREACTColor(201, 192, 187, 255);
        private static readonly PREACTColor PaleSpringBud = new PREACTColor(236, 235, 189, 255);
        private static readonly PREACTColor PaleTaupe = new PREACTColor(188, 152, 126, 255);
        private static readonly PREACTColor PaleViolet = new PREACTColor(204, 153, 255, 255);
        private static readonly PREACTColor PalmLeaf = new PREACTColor(111, 153, 64, 255);
        private static readonly PREACTColor PansyPurple = new PREACTColor(120, 24, 74, 255);
        private static readonly PREACTColor PaoloVeroneseGreen = new PREACTColor(0, 155, 125, 255);
        private static readonly PREACTColor PapayaWhip = new PREACTColor(255, 239, 213, 255);
        private static readonly PREACTColor ParadisePink = new PREACTColor(230, 62, 98, 255);
        private static readonly PREACTColor ParrotPink = new PREACTColor(217, 152, 160, 255);
        private static readonly PREACTColor PastelBlue = new PREACTColor(174, 198, 207, 255);
        private static readonly PREACTColor PastelBrown = new PREACTColor(130, 105, 83, 255);
        private static readonly PREACTColor PastelGray = new PREACTColor(207, 207, 196, 255);
        private static readonly PREACTColor PastelGreen = new PREACTColor(119, 221, 119, 255);
        private static readonly PREACTColor PastelMagenta = new PREACTColor(244, 154, 194, 255);
        private static readonly PREACTColor PastelOrange = new PREACTColor(255, 179, 71, 255);
        private static readonly PREACTColor PastelPink = new PREACTColor(222, 165, 164, 255);
        private static readonly PREACTColor PastelPurple = new PREACTColor(179, 158, 181, 255);
        private static readonly PREACTColor PastelRed = new PREACTColor(255, 105, 97, 255);
        private static readonly PREACTColor PastelViolet = new PREACTColor(203, 153, 201, 255);
        private static readonly PREACTColor PastelYellow = new PREACTColor(253, 253, 150, 255);
        private static readonly PREACTColor Patriarch = new PREACTColor(128, 0, 128, 255);
        private static readonly PREACTColor Peach = new PREACTColor(255, 229, 180, 255);
        private static readonly PREACTColor PeachOrange = new PREACTColor(255, 204, 153, 255);
        private static readonly PREACTColor PeachPuff = new PREACTColor(255, 218, 185, 255);
        private static readonly PREACTColor PeachYellow = new PREACTColor(250, 223, 173, 255);
        private static readonly PREACTColor Pear = new PREACTColor(209, 226, 49, 255);
        private static readonly PREACTColor Pearl = new PREACTColor(234, 224, 200, 255);
        private static readonly PREACTColor PearlAqua = new PREACTColor(136, 216, 192, 255);
        private static readonly PREACTColor PearlyPurple = new PREACTColor(183, 104, 162, 255);
        private static readonly PREACTColor Peridot = new PREACTColor(230, 226, 0, 255);
        private static readonly PREACTColor PeriwinkleCrayola = new PREACTColor(195, 205, 230, 255);
        private static readonly PREACTColor PermanentGeraniumLake = new PREACTColor(225, 44, 44, 255);
        private static readonly PREACTColor PersianBlue = new PREACTColor(28, 57, 187, 255);
        private static readonly PREACTColor PersianGreen = new PREACTColor(0, 166, 147, 255);
        private static readonly PREACTColor PersianIndigo = new PREACTColor(50, 18, 122, 255);
        private static readonly PREACTColor PersianOrange = new PREACTColor(217, 144, 88, 255);
        private static readonly PREACTColor PersianPink = new PREACTColor(247, 127, 190, 255);
        private static readonly PREACTColor PersianPlum = new PREACTColor(112, 28, 28, 255);
        private static readonly PREACTColor PersianRed = new PREACTColor(204, 51, 51, 255);
        private static readonly PREACTColor PersianRose = new PREACTColor(254, 40, 162, 255);
        private static readonly PREACTColor Persimmon = new PREACTColor(236, 88, 0, 255);
        private static readonly PREACTColor Peru = new PREACTColor(205, 133, 63, 255);
        private static readonly PREACTColor PewterBlue = new PREACTColor(139, 168, 183, 255);
        private static readonly PREACTColor PhilippineBlue = new PREACTColor(0, 56, 167, 255);
        private static readonly PREACTColor PhilippineBrown = new PREACTColor(93, 25, 22, 255);
        private static readonly PREACTColor PhilippineGold = new PREACTColor(177, 115, 4, 255);
        private static readonly PREACTColor PhilippineGoldenYellow = new PREACTColor(253, 223, 22, 255);
        private static readonly PREACTColor PhilippineGray = new PREACTColor(140, 140, 140, 255);
        private static readonly PREACTColor PhilippineGreen = new PREACTColor(0, 133, 67, 255);
        private static readonly PREACTColor PhilippineOrange = new PREACTColor(255, 115, 0, 255);
        private static readonly PREACTColor PhilippinePink = new PREACTColor(255, 26, 142, 255);
        private static readonly PREACTColor PhilippineRed = new PREACTColor(206, 17, 39, 255);
        private static readonly PREACTColor PhilippineSilver = new PREACTColor(179, 179, 179, 255);
        private static readonly PREACTColor PhilippineViolet = new PREACTColor(129, 0, 127, 255);
        private static readonly PREACTColor PhilippineYellow = new PREACTColor(254, 203, 0, 255);
        private static readonly PREACTColor Phlox = new PREACTColor(223, 0, 255, 255);
        private static readonly PREACTColor PhthaloBlue = new PREACTColor(0, 15, 137, 255);
        private static readonly PREACTColor PhthaloGreen = new PREACTColor(18, 53, 36, 255);
        private static readonly PREACTColor PictonBlue = new PREACTColor(69, 177, 232, 255);
        private static readonly PREACTColor PictorialCarmine = new PREACTColor(195, 11, 78, 255);
        private static readonly PREACTColor PiggyPink = new PREACTColor(253, 221, 230, 255);
        private static readonly PREACTColor PineGreen = new PREACTColor(1, 121, 111, 255);
        private static readonly PREACTColor PineTree = new PREACTColor(42, 47, 35, 255);
        private static readonly PREACTColor Pineapple = new PREACTColor(86, 60, 13, 255);
        private static readonly PREACTColor Pink = new PREACTColor(255, 192, 203, 255);
        private static readonly PREACTColor PinkPantone = new PREACTColor(215, 72, 148, 255);
        private static readonly PREACTColor PinkFlamingo = new PREACTColor(252, 116, 253, 255);
        private static readonly PREACTColor PinkLace = new PREACTColor(255, 221, 244, 255);
        private static readonly PREACTColor PinkLavender = new PREACTColor(216, 178, 209, 255);
        private static readonly PREACTColor PinkPearl = new PREACTColor(231, 172, 207, 255);
        private static readonly PREACTColor PinkRaspberry = new PREACTColor(152, 0, 54, 255);
        private static readonly PREACTColor PinkSherbet = new PREACTColor(247, 143, 167, 255);
        private static readonly PREACTColor Pistachio = new PREACTColor(147, 197, 114, 255);
        private static readonly PREACTColor PixiePowder = new PREACTColor(57, 18, 133, 255);
        private static readonly PREACTColor Platinum = new PREACTColor(229, 228, 226, 255);
        private static readonly PREACTColor Plum = new PREACTColor(142, 69, 133, 255);
        private static readonly PREACTColor PlumpPurple = new PREACTColor(89, 70, 178, 255);
        private static readonly PREACTColor PoliceBlue = new PREACTColor(55, 79, 107, 255);
        private static readonly PREACTColor PolishedPine = new PREACTColor(93, 164, 147, 255);
        private static readonly PREACTColor Popstar = new PREACTColor(190, 79, 98, 255);
        private static readonly PREACTColor PortlandOrange = new PREACTColor(255, 90, 54, 255);
        private static readonly PREACTColor PowderBlue = new PREACTColor(176, 224, 230, 255);
        private static readonly PREACTColor PrincessPerfume = new PREACTColor(255, 133, 207, 255);
        private static readonly PREACTColor PrincetonOrange = new PREACTColor(245, 128, 37, 255);
        private static readonly PREACTColor PrussianBlue = new PREACTColor(0, 49, 83, 255);
        private static readonly PREACTColor Puce = new PREACTColor(204, 136, 153, 255);
        private static readonly PREACTColor PuceRed = new PREACTColor(114, 47, 55, 255);
        private static readonly PREACTColor PullmanBrownUPSBrown = new PREACTColor(100, 65, 23, 255);
        private static readonly PREACTColor PullmanGreen = new PREACTColor(59, 51, 28, 255);
        private static readonly PREACTColor Pumpkin = new PREACTColor(255, 117, 24, 255);
        private static readonly PREACTColor PurpleMunsell = new PREACTColor(159, 0, 197, 255);
        private static readonly PREACTColor PurpleX11 = new PREACTColor(160, 32, 240, 255);
        private static readonly PREACTColor PurpleHeart = new PREACTColor(105, 53, 156, 255);
        private static readonly PREACTColor PurpleMountainMajesty = new PREACTColor(150, 120, 182, 255);
        private static readonly PREACTColor PurpleNavy = new PREACTColor(78, 81, 128, 255);
        private static readonly PREACTColor PurplePizzazz = new PREACTColor(254, 78, 218, 255);
        private static readonly PREACTColor PurplePlum = new PREACTColor(156, 81, 182, 255);
        private static readonly PREACTColor PurpleTaupe = new PREACTColor(80, 64, 77, 255);
        private static readonly PREACTColor Purpureus = new PREACTColor(154, 78, 174, 255);
        private static readonly PREACTColor Quartz = new PREACTColor(81, 72, 79, 255);
        private static readonly PREACTColor QueenBlue = new PREACTColor(67, 107, 149, 255);
        private static readonly PREACTColor QueenPink = new PREACTColor(232, 204, 215, 255);
        private static readonly PREACTColor QuickSilver = new PREACTColor(166, 166, 166, 255);
        private static readonly PREACTColor QuinacridoneMagenta = new PREACTColor(142, 58, 89, 255);
        private static readonly PREACTColor Quincy = new PREACTColor(106, 84, 69, 255);
        private static readonly PREACTColor RadicalRed = new PREACTColor(255, 53, 94, 255);
        private static readonly PREACTColor RaisinBlack = new PREACTColor(36, 33, 36, 255);
        private static readonly PREACTColor Rajah = new PREACTColor(251, 171, 96, 255);
        private static readonly PREACTColor Raspberry = new PREACTColor(227, 11, 92, 255);
        private static readonly PREACTColor RaspberryPink = new PREACTColor(226, 80, 152, 255);
        private static readonly PREACTColor RawSienna = new PREACTColor(214, 138, 89, 255);
        private static readonly PREACTColor RawUmber = new PREACTColor(130, 102, 68, 255);
        private static readonly PREACTColor RazzleDazzleRose = new PREACTColor(255, 51, 204, 255);
        private static readonly PREACTColor Razzmatazz = new PREACTColor(227, 37, 107, 255);
        private static readonly PREACTColor RazzmicBerry = new PREACTColor(141, 78, 133, 255);
        private static readonly PREACTColor RebeccaPurple = new PREACTColor(102, 52, 153, 255);
        private static readonly PREACTColor Red = new PREACTColor(255, 0, 0, 255);
        private static readonly PREACTColor RedCrayola = new PREACTColor(238, 32, 77, 255);
        private static readonly PREACTColor RedMunsell = new PREACTColor(242, 0, 60, 255);
        private static readonly PREACTColor RedNCS = new PREACTColor(196, 2, 51, 255);
        private static readonly PREACTColor RedPigment = new PREACTColor(237, 28, 36, 255);
        private static readonly PREACTColor RedRYB = new PREACTColor(254, 39, 18, 255);
        private static readonly PREACTColor RedDevil = new PREACTColor(134, 1, 17, 255);
        private static readonly PREACTColor RedOrange = new PREACTColor(255, 83, 73, 255);
        private static readonly PREACTColor RedPurple = new PREACTColor(228, 0, 120, 255);
        private static readonly PREACTColor RedSalsa = new PREACTColor(253, 58, 74, 255);
        private static readonly PREACTColor Redwood = new PREACTColor(164, 90, 82, 255);
        private static readonly PREACTColor Regalia = new PREACTColor(82, 45, 128, 255);
        private static readonly PREACTColor ResolutionBlue = new PREACTColor(0, 35, 135, 255);
        private static readonly PREACTColor Rhythm = new PREACTColor(119, 118, 150, 255);
        private static readonly PREACTColor RichBlack = new PREACTColor(0, 64, 64, 255);
        private static readonly PREACTColor RichBlackFOGRA29 = new PREACTColor(1, 11, 19, 255);
        private static readonly PREACTColor RichBlackFOGRA39 = new PREACTColor(1, 2, 3, 255);
        private static readonly PREACTColor RichBrilliantLavender = new PREACTColor(241, 167, 254, 255);
        private static readonly PREACTColor RichElectricBlue = new PREACTColor(8, 146, 208, 255);
        private static readonly PREACTColor RichLavender = new PREACTColor(167, 107, 207, 255);
        private static readonly PREACTColor RichLilac = new PREACTColor(182, 102, 210, 255);
        private static readonly PREACTColor RifleGreen = new PREACTColor(68, 76, 56, 255);
        private static readonly PREACTColor RobinEggBlue = new PREACTColor(0, 204, 204, 255);
        private static readonly PREACTColor RocketMetallic = new PREACTColor(138, 127, 128, 255);
        private static readonly PREACTColor RomanSilver = new PREACTColor(131, 137, 150, 255);
        private static readonly PREACTColor RootBeer = new PREACTColor(41, 14, 5, 255);
        private static readonly PREACTColor RoseBonbon = new PREACTColor(249, 66, 158, 255);
        private static readonly PREACTColor RoseDust = new PREACTColor(158, 94, 111, 255);
        private static readonly PREACTColor RoseEbony = new PREACTColor(103, 72, 70, 255);
        private static readonly PREACTColor RoseGarnet = new PREACTColor(150, 1, 69, 255);
        private static readonly PREACTColor RoseGold = new PREACTColor(183, 110, 121, 255);
        private static readonly PREACTColor RosePink = new PREACTColor(255, 102, 204, 255);
        private static readonly PREACTColor RoseQuartz = new PREACTColor(170, 152, 169, 255);
        private static readonly PREACTColor RoseQuartzPink = new PREACTColor(189, 85, 156, 255);
        private static readonly PREACTColor RoseRed = new PREACTColor(194, 30, 86, 255);
        private static readonly PREACTColor RoseTaupe = new PREACTColor(144, 93, 93, 255);
        private static readonly PREACTColor RoseVale = new PREACTColor(171, 78, 82, 255);
        private static readonly PREACTColor Rosewood = new PREACTColor(101, 0, 11, 255);
        private static readonly PREACTColor RossoCorsa = new PREACTColor(212, 0, 0, 255);
        private static readonly PREACTColor RosyBrown = new PREACTColor(188, 143, 143, 255);
        private static readonly PREACTColor RoyalAzure = new PREACTColor(0, 56, 168, 255);
        private static readonly PREACTColor RoyalBlue = new PREACTColor(0, 35, 102, 255);
        private static readonly PREACTColor RoyalBlue2 = new PREACTColor(65, 105, 225, 255);
        private static readonly PREACTColor RoyalBrown = new PREACTColor(82, 59, 53, 255);
        private static readonly PREACTColor RoyalFuchsia = new PREACTColor(202, 44, 146, 255);
        private static readonly PREACTColor RoyalGreen = new PREACTColor(19, 98, 7, 255);
        private static readonly PREACTColor RoyalOrange = new PREACTColor(249, 146, 69, 255);
        private static readonly PREACTColor RoyalPink = new PREACTColor(231, 56, 149, 255);
        private static readonly PREACTColor RoyalRed = new PREACTColor(155, 28, 49, 255);
        private static readonly PREACTColor RoyalRed2 = new PREACTColor(208, 0, 96, 255);
        private static readonly PREACTColor RoyalPurple = new PREACTColor(120, 81, 169, 255);
        private static readonly PREACTColor Ruber = new PREACTColor(206, 70, 118, 255);
        private static readonly PREACTColor RubineRed = new PREACTColor(209, 0, 86, 255);
        private static readonly PREACTColor Ruby = new PREACTColor(224, 17, 95, 255);
        private static readonly PREACTColor RubyRed = new PREACTColor(155, 17, 30, 255);
        private static readonly PREACTColor Ruddy = new PREACTColor(255, 0, 40, 255);
        private static readonly PREACTColor RuddyBrown = new PREACTColor(187, 101, 40, 255);
        private static readonly PREACTColor RuddyPink = new PREACTColor(225, 142, 150, 255);
        private static readonly PREACTColor Rufous = new PREACTColor(168, 28, 7, 255);
        private static readonly PREACTColor Russet = new PREACTColor(128, 70, 27, 255);
        private static readonly PREACTColor RussianGreen = new PREACTColor(103, 146, 103, 255);
        private static readonly PREACTColor RussianViolet = new PREACTColor(50, 23, 77, 255);
        private static readonly PREACTColor Rust = new PREACTColor(183, 65, 14, 255);
        private static readonly PREACTColor RustyRed = new PREACTColor(218, 44, 67, 255);
        private static readonly PREACTColor SacramentoStateGreen = new PREACTColor(4, 57, 39, 255);
        private static readonly PREACTColor SaddleBrown = new PREACTColor(139, 69, 19, 255);
        private static readonly PREACTColor SafetyOrange = new PREACTColor(255, 120, 0, 255);
        private static readonly PREACTColor SafetyOrangeBlazeOrange = new PREACTColor(255, 103, 0, 255);
        private static readonly PREACTColor SafetyYellow = new PREACTColor(238, 210, 2, 255);
        private static readonly PREACTColor Saffron = new PREACTColor(244, 196, 48, 255);
        private static readonly PREACTColor Sage = new PREACTColor(188, 184, 138, 255);
        private static readonly PREACTColor StPatricksBlue = new PREACTColor(35, 41, 122, 255);
        private static readonly PREACTColor SalemColor = new PREACTColor(23, 123, 77, 255);
        private static readonly PREACTColor Salmon = new PREACTColor(250, 128, 114, 255);
        private static readonly PREACTColor SalmonPink = new PREACTColor(255, 145, 164, 255);
        private static readonly PREACTColor Sandstorm = new PREACTColor(236, 213, 64, 255);
        private static readonly PREACTColor SandyBrown = new PREACTColor(244, 164, 96, 255);
        private static readonly PREACTColor SandyTan = new PREACTColor(253, 217, 181, 255);
        private static readonly PREACTColor Sangria = new PREACTColor(146, 0, 10, 255);
        private static readonly PREACTColor SapGreen = new PREACTColor(80, 125, 42, 255);
        private static readonly PREACTColor Sapphire = new PREACTColor(15, 82, 186, 255);
        private static readonly PREACTColor SasquatchSocks = new PREACTColor(255, 70, 129, 255);
        private static readonly PREACTColor SatinSheenGold = new PREACTColor(203, 161, 53, 255);
        private static readonly PREACTColor Scarlet = new PREACTColor(255, 36, 0, 255);
        private static readonly PREACTColor Scarlet2 = new PREACTColor(253, 14, 53, 255);
        private static readonly PREACTColor SchoolBusYellow = new PREACTColor(255, 216, 0, 255);
        private static readonly PREACTColor ScreaminGreen = new PREACTColor(102, 255, 102, 255);
        private static readonly PREACTColor SeaBlue = new PREACTColor(0, 105, 148, 255);
        private static readonly PREACTColor SeaFoamGreen = new PREACTColor(195, 226, 191, 255);
        private static readonly PREACTColor SeaGreen = new PREACTColor(46, 139, 87, 255);
        private static readonly PREACTColor SeaGreenCrayola = new PREACTColor(1, 255, 205, 255);
        private static readonly PREACTColor SeaSerpent = new PREACTColor(75, 199, 207, 255);
        private static readonly PREACTColor SealBrown = new PREACTColor(50, 20, 20, 255);
        private static readonly PREACTColor Seashell = new PREACTColor(255, 245, 238, 255);
        private static readonly PREACTColor SelectiveYellow = new PREACTColor(255, 186, 0, 255);
        private static readonly PREACTColor Sepia = new PREACTColor(112, 66, 20, 255);
        private static readonly PREACTColor Shadow = new PREACTColor(138, 121, 93, 255);
        private static readonly PREACTColor ShadowBlue = new PREACTColor(119, 139, 165, 255);
        private static readonly PREACTColor Shampoo = new PREACTColor(255, 207, 241, 255);
        private static readonly PREACTColor ShamrockGreen = new PREACTColor(0, 158, 96, 255);
        private static readonly PREACTColor SheenGreen = new PREACTColor(143, 212, 0, 255);
        private static readonly PREACTColor ShimmeringBlush = new PREACTColor(217, 134, 149, 255);
        private static readonly PREACTColor ShinyShamrock = new PREACTColor(95, 167, 120, 255);
        private static readonly PREACTColor ShockingPink = new PREACTColor(252, 15, 192, 255);
        private static readonly PREACTColor ShockingPinkCrayola = new PREACTColor(255, 111, 255, 255);
        private static readonly PREACTColor Silver = new PREACTColor(192, 192, 192, 255);
        private static readonly PREACTColor SilverMetallic = new PREACTColor(170, 169, 173, 255);
        private static readonly PREACTColor SilverChalice = new PREACTColor(172, 172, 172, 255);
        private static readonly PREACTColor SilverFoil = new PREACTColor(175, 177, 174, 255);
        private static readonly PREACTColor SilverLakeBlue = new PREACTColor(93, 137, 186, 255);
        private static readonly PREACTColor SilverPink = new PREACTColor(196, 174, 173, 255);
        private static readonly PREACTColor SilverSand = new PREACTColor(191, 193, 194, 255);
        private static readonly PREACTColor Sinopia = new PREACTColor(203, 65, 11, 255);
        private static readonly PREACTColor SizzlingRed = new PREACTColor(255, 56, 85, 255);
        private static readonly PREACTColor SizzlingSunrise = new PREACTColor(255, 219, 0, 255);
        private static readonly PREACTColor Skobeloff = new PREACTColor(0, 116, 116, 255);
        private static readonly PREACTColor SkyBlue = new PREACTColor(135, 206, 235, 255);
        private static readonly PREACTColor SkyBlueCrayola = new PREACTColor(118, 215, 234, 255);
        private static readonly PREACTColor SkyMagenta = new PREACTColor(207, 113, 175, 255);
        private static readonly PREACTColor SlateBlue = new PREACTColor(106, 90, 205, 255);
        private static readonly PREACTColor SlateGray = new PREACTColor(112, 128, 144, 255);
        private static readonly PREACTColor SlimyGreen = new PREACTColor(41, 150, 23, 255);
        private static readonly PREACTColor SmashedPumpkin = new PREACTColor(255, 109, 58, 255);
        private static readonly PREACTColor Smitten = new PREACTColor(200, 65, 134, 255);
        private static readonly PREACTColor Smoke = new PREACTColor(115, 130, 118, 255);
        private static readonly PREACTColor SmokeyTopaz = new PREACTColor(131, 42, 34, 255);
        private static readonly PREACTColor SmokyBlack = new PREACTColor(16, 12, 8, 255);
        private static readonly PREACTColor SmokyTopaz = new PREACTColor(147, 61, 65, 255);
        private static readonly PREACTColor Snow = new PREACTColor(255, 250, 250, 255);
        private static readonly PREACTColor Soap = new PREACTColor(206, 200, 239, 255);
        private static readonly PREACTColor SoldierGreen = new PREACTColor(84, 90, 44, 255);
        private static readonly PREACTColor SolidPink = new PREACTColor(137, 56, 67, 255);
        private static readonly PREACTColor SonicSilver = new PREACTColor(117, 117, 117, 255);
        private static readonly PREACTColor SpartanCrimson = new PREACTColor(158, 19, 22, 255);
        private static readonly PREACTColor SpaceCadet = new PREACTColor(29, 41, 81, 255);
        private static readonly PREACTColor SpanishBistre = new PREACTColor(128, 117, 50, 255);
        private static readonly PREACTColor SpanishBlue = new PREACTColor(0, 112, 184, 255);
        private static readonly PREACTColor SpanishCarmine = new PREACTColor(209, 0, 71, 255);
        private static readonly PREACTColor SpanishCrimson = new PREACTColor(229, 26, 76, 255);
        private static readonly PREACTColor SpanishGray = new PREACTColor(152, 152, 152, 255);
        private static readonly PREACTColor SpanishGreen = new PREACTColor(0, 145, 80, 255);
        private static readonly PREACTColor SpanishOrange = new PREACTColor(232, 97, 0, 255);
        private static readonly PREACTColor SpanishPink = new PREACTColor(247, 191, 190, 255);
        private static readonly PREACTColor SpanishPurple = new PREACTColor(102, 3, 60, 255);
        private static readonly PREACTColor SpanishRed = new PREACTColor(230, 0, 38, 255);
        private static readonly PREACTColor SpanishViolet = new PREACTColor(76, 40, 130, 255);
        private static readonly PREACTColor SpanishViridian = new PREACTColor(0, 127, 92, 255);
        private static readonly PREACTColor SpanishYellow = new PREACTColor(246, 181, 17, 255);
        private static readonly PREACTColor SpicyMix = new PREACTColor(139, 95, 77, 255);
        private static readonly PREACTColor SpiroDiscoBall = new PREACTColor(15, 192, 252, 255);
        private static readonly PREACTColor SpringBud = new PREACTColor(167, 252, 0, 255);
        private static readonly PREACTColor SpringFrost = new PREACTColor(135, 255, 42, 255);
        private static readonly PREACTColor StarCommandBlue = new PREACTColor(0, 123, 184, 255);
        private static readonly PREACTColor SteelBlue = new PREACTColor(70, 130, 180, 255);
        private static readonly PREACTColor SteelPink = new PREACTColor(204, 51, 204, 255);
        private static readonly PREACTColor SteelTeal = new PREACTColor(95, 138, 139, 255);
        private static readonly PREACTColor Stormcloud = new PREACTColor(79, 102, 106, 255);
        private static readonly PREACTColor Straw = new PREACTColor(228, 217, 111, 255);
        private static readonly PREACTColor Strawberry = new PREACTColor(252, 90, 141, 255);
        private static readonly PREACTColor SugarPlum = new PREACTColor(145, 78, 117, 255);
        private static readonly PREACTColor SunburntCyclops = new PREACTColor(255, 64, 76, 255);
        private static readonly PREACTColor Sunglow = new PREACTColor(255, 204, 51, 255);
        private static readonly PREACTColor Sunny = new PREACTColor(242, 242, 122, 255);
        private static readonly PREACTColor Sunray = new PREACTColor(227, 171, 87, 255);
        private static readonly PREACTColor SunsetOrange = new PREACTColor(253, 94, 83, 255);
        private static readonly PREACTColor SuperPink = new PREACTColor(207, 107, 169, 255);
        private static readonly PREACTColor SweetBrown = new PREACTColor(168, 55, 49, 255);
        private static readonly PREACTColor Tan = new PREACTColor(210, 180, 140, 255);
        private static readonly PREACTColor Tangelo = new PREACTColor(249, 77, 0, 255);
        private static readonly PREACTColor Tangerine = new PREACTColor(242, 133, 0, 255);
        private static readonly PREACTColor TartOrange = new PREACTColor(251, 77, 70, 255);
        private static readonly PREACTColor TaupeGray = new PREACTColor(139, 133, 137, 255);
        private static readonly PREACTColor TeaGreen = new PREACTColor(208, 240, 192, 255);
        private static readonly PREACTColor Teal = new PREACTColor(0, 128, 128, 255);
        private static readonly PREACTColor TealBlue = new PREACTColor(54, 117, 136, 255);
        private static readonly PREACTColor TealDeer = new PREACTColor(153, 230, 179, 255);
        private static readonly PREACTColor TealGreen = new PREACTColor(0, 130, 127, 255);
        private static readonly PREACTColor Telemagenta = new PREACTColor(207, 52, 118, 255);
        private static readonly PREACTColor Temptress = new PREACTColor(60, 33, 38, 255);
        private static readonly PREACTColor TennéTawny = new PREACTColor(205, 87, 0, 255);
        private static readonly PREACTColor TerraCotta = new PREACTColor(226, 114, 91, 255);
        private static readonly PREACTColor Thistle = new PREACTColor(216, 191, 216, 255);
        private static readonly PREACTColor TickleMePink = new PREACTColor(252, 137, 172, 255);
        private static readonly PREACTColor TiffanyBlue = new PREACTColor(10, 186, 181, 255);
        private static readonly PREACTColor TigersEye = new PREACTColor(224, 141, 60, 255);
        private static readonly PREACTColor Timberwolf = new PREACTColor(219, 215, 210, 255);
        private static readonly PREACTColor Titanium = new PREACTColor(135, 134, 129, 255);
        private static readonly PREACTColor TitaniumYellow = new PREACTColor(238, 230, 0, 255);
        private static readonly PREACTColor Tomato = new PREACTColor(255, 99, 71, 255);
        private static readonly PREACTColor Toolbox = new PREACTColor(116, 108, 192, 255);
        private static readonly PREACTColor Topaz = new PREACTColor(255, 200, 124, 255);
        private static readonly PREACTColor TropicalRainForest = new PREACTColor(0, 117, 94, 255);
        private static readonly PREACTColor TropicalViolet = new PREACTColor(205, 164, 222, 255);
        private static readonly PREACTColor TrueBlue = new PREACTColor(0, 115, 207, 255);
        private static readonly PREACTColor TuftsBlue = new PREACTColor(62, 142, 222, 255);
        private static readonly PREACTColor Tulip = new PREACTColor(255, 135, 141, 255);
        private static readonly PREACTColor Tumbleweed = new PREACTColor(222, 170, 136, 255);
        private static readonly PREACTColor TurkishRose = new PREACTColor(181, 114, 129, 255);
        private static readonly PREACTColor Turquoise = new PREACTColor(64, 224, 208, 255);
        private static readonly PREACTColor TurquoiseBlue = new PREACTColor(0, 255, 239, 255);
        private static readonly PREACTColor TurquoiseGreen = new PREACTColor(160, 214, 180, 255);
        private static readonly PREACTColor TurquoiseSurf = new PREACTColor(0, 197, 205, 255);
        private static readonly PREACTColor TuscanRed = new PREACTColor(124, 72, 72, 255);
        private static readonly PREACTColor Tuscany = new PREACTColor(192, 153, 153, 255);
        private static readonly PREACTColor TwilightLavender = new PREACTColor(138, 73, 107, 255);
        private static readonly PREACTColor UABlue = new PREACTColor(0, 51, 170, 255);
        private static readonly PREACTColor UARed = new PREACTColor(217, 0, 76, 255);
        private static readonly PREACTColor Ube = new PREACTColor(136, 120, 195, 255);
        private static readonly PREACTColor UCLABlue = new PREACTColor(83, 104, 149, 255);
        private static readonly PREACTColor UCLAGold = new PREACTColor(255, 179, 0, 255);
        private static readonly PREACTColor UERed = new PREACTColor(186, 0, 1, 255);
        private static readonly PREACTColor UFOGreen = new PREACTColor(60, 208, 112, 255);
        private static readonly PREACTColor Ultramarine = new PREACTColor(18, 10, 143, 255);
        private static readonly PREACTColor UltramarineBlue = new PREACTColor(65, 102, 245, 255);
        private static readonly PREACTColor UltraRed = new PREACTColor(252, 108, 133, 255);
        private static readonly PREACTColor Umber = new PREACTColor(99, 81, 71, 255);
        private static readonly PREACTColor UnbleachedSilk = new PREACTColor(255, 221, 202, 255);
        private static readonly PREACTColor UnitedNationsBlue = new PREACTColor(91, 146, 229, 255);
        private static readonly PREACTColor UniversityOfCaliforniaGold = new PREACTColor(183, 135, 39, 255);
        private static readonly PREACTColor UniversityOfTennesseeOrange = new PREACTColor(247, 127, 0, 255);
        private static readonly PREACTColor UPMaroon = new PREACTColor(123, 17, 19, 255);
        private static readonly PREACTColor UpsdellRed = new PREACTColor(174, 32, 41, 255);
        private static readonly PREACTColor Urobilin = new PREACTColor(225, 173, 33, 255);
        private static readonly PREACTColor USAFABlue = new PREACTColor(0, 79, 152, 255);
        private static readonly PREACTColor UtahCrimson = new PREACTColor(211, 0, 63, 255);
        private static readonly PREACTColor VampireBlack = new PREACTColor(8, 8, 8, 255);
        private static readonly PREACTColor VanDykeBrown = new PREACTColor(102, 66, 40, 255);
        private static readonly PREACTColor VanillaIce = new PREACTColor(243, 143, 169, 255);
        private static readonly PREACTColor VegasGold = new PREACTColor(197, 179, 88, 255);
        private static readonly PREACTColor VenetianRed = new PREACTColor(200, 8, 21, 255);
        private static readonly PREACTColor Verdigris = new PREACTColor(67, 179, 174, 255);
        private static readonly PREACTColor Vermilion = new PREACTColor(217, 56, 30, 255);
        private static readonly PREACTColor VerseGreen = new PREACTColor(24, 136, 13, 255);
        private static readonly PREACTColor VeryLightAzure = new PREACTColor(116, 187, 251, 255);
        private static readonly PREACTColor VeryLightBlue = new PREACTColor(102, 102, 255, 255);
        private static readonly PREACTColor VeryLightMalachiteGreen = new PREACTColor(100, 233, 134, 255);
        private static readonly PREACTColor VeryLightTangelo = new PREACTColor(255, 176, 119, 255);
        private static readonly PREACTColor VeryPaleOrange = new PREACTColor(255, 223, 191, 255);
        private static readonly PREACTColor VeryPaleYellow = new PREACTColor(255, 255, 191, 255);
        private static readonly PREACTColor VioletColorWheel = new PREACTColor(127, 0, 255, 255);
        private static readonly PREACTColor VioletCrayola = new PREACTColor(150, 61, 127, 255);
        private static readonly PREACTColor VioletRYB = new PREACTColor(134, 1, 175, 255);
        private static readonly PREACTColor VioletBlue = new PREACTColor(50, 74, 178, 255);
        private static readonly PREACTColor VioletRed = new PREACTColor(247, 83, 148, 255);
        private static readonly PREACTColor ViolinBrown = new PREACTColor(103, 68, 3, 255);
        private static readonly PREACTColor ViridianGreen = new PREACTColor(0, 150, 152, 255);
        private static readonly PREACTColor VistaBlue = new PREACTColor(124, 158, 217, 255);
        private static readonly PREACTColor VividAuburn = new PREACTColor(146, 39, 36, 255);
        private static readonly PREACTColor VividBurgundy = new PREACTColor(159, 29, 53, 255);
        private static readonly PREACTColor VividCerise = new PREACTColor(218, 29, 129, 255);
        private static readonly PREACTColor VividCerulean = new PREACTColor(0, 170, 238, 255);
        private static readonly PREACTColor VividCrimson = new PREACTColor(204, 0, 51, 255);
        private static readonly PREACTColor VividGamboge = new PREACTColor(255, 153, 0, 255);
        private static readonly PREACTColor VividLimeGreen = new PREACTColor(166, 214, 8, 255);
        private static readonly PREACTColor VividMalachite = new PREACTColor(0, 204, 51, 255);
        private static readonly PREACTColor VividMulberry = new PREACTColor(184, 12, 227, 255);
        private static readonly PREACTColor VividOrange = new PREACTColor(255, 95, 0, 255);
        private static readonly PREACTColor VividOrangePeel = new PREACTColor(255, 160, 0, 255);
        private static readonly PREACTColor VividOrchid = new PREACTColor(204, 0, 255, 255);
        private static readonly PREACTColor VividRaspberry = new PREACTColor(255, 0, 108, 255);
        private static readonly PREACTColor VividRed = new PREACTColor(247, 13, 26, 255);
        private static readonly PREACTColor VividRedTangelo = new PREACTColor(223, 97, 36, 255);
        private static readonly PREACTColor VividSkyBlue = new PREACTColor(0, 204, 255, 255);
        private static readonly PREACTColor VividTangelo = new PREACTColor(240, 116, 39, 255);
        private static readonly PREACTColor VividTangerine = new PREACTColor(255, 160, 137, 255);
        private static readonly PREACTColor VividVermilion = new PREACTColor(229, 96, 36, 255);
        private static readonly PREACTColor VividViolet = new PREACTColor(159, 0, 255, 255);
        private static readonly PREACTColor VividYellow = new PREACTColor(255, 227, 2, 255);
        private static readonly PREACTColor Volt = new PREACTColor(205, 255, 0, 255);
        private static readonly PREACTColor WageningenGreen = new PREACTColor(52, 178, 51, 255);
        private static readonly PREACTColor WarmBlack = new PREACTColor(0, 66, 66, 255);
        private static readonly PREACTColor Watermelon = new PREACTColor(240, 92, 133, 255);
        private static readonly PREACTColor WatermelonRed = new PREACTColor(190, 65, 71, 255);
        private static readonly PREACTColor Waterspout = new PREACTColor(164, 244, 249, 255);
        private static readonly PREACTColor WeldonBlue = new PREACTColor(124, 152, 171, 255);
        private static readonly PREACTColor Wenge = new PREACTColor(100, 84, 82, 255);
        private static readonly PREACTColor Wheat = new PREACTColor(245, 222, 179, 255);
        private static readonly PREACTColor White = new PREACTColor(255, 255, 255, 255);
        private static readonly PREACTColor WhiteChocolate = new PREACTColor(237, 230, 214, 255);
        private static readonly PREACTColor WhiteCoffee = new PREACTColor(230, 224, 212, 255);
        private static readonly PREACTColor WildBlueYonder = new PREACTColor(162, 173, 208, 255);
        private static readonly PREACTColor WildOrchid = new PREACTColor(212, 112, 162, 255);
        private static readonly PREACTColor WildStrawberry = new PREACTColor(255, 67, 164, 255);
        private static readonly PREACTColor WillpowerOrange = new PREACTColor(253, 88, 0, 255);
        private static readonly PREACTColor WindsorTan = new PREACTColor(167, 85, 2, 255);
        private static readonly PREACTColor WineRed = new PREACTColor(177, 18, 38, 255);
        private static readonly PREACTColor WinterSky = new PREACTColor(255, 0, 124, 255);
        private static readonly PREACTColor WinterWizard = new PREACTColor(160, 230, 255, 255);
        private static readonly PREACTColor WintergreenDream = new PREACTColor(86, 136, 125, 255);
        private static readonly PREACTColor Wisteria = new PREACTColor(201, 160, 220, 255);
        private static readonly PREACTColor Xanadu = new PREACTColor(115, 134, 120, 255);
        private static readonly PREACTColor YaleBlue = new PREACTColor(15, 77, 146, 255);
        private static readonly PREACTColor YankeesBlue = new PREACTColor(28, 40, 65, 255);
        private static readonly PREACTColor Yellow = new PREACTColor(255, 255, 0, 255);
        private static readonly PREACTColor YellowCrayola = new PREACTColor(252, 232, 131, 255);
        private static readonly PREACTColor YellowMunsell = new PREACTColor(239, 204, 0, 255);
        private static readonly PREACTColor YellowPantone = new PREACTColor(254, 223, 0, 255);
        private static readonly PREACTColor YellowRYB = new PREACTColor(254, 254, 51, 255);
        private static readonly PREACTColor YellowGreen = new PREACTColor(154, 205, 50, 255);
        private static readonly PREACTColor YellowOrange = new PREACTColor(255, 174, 66, 255);
        private static readonly PREACTColor YellowRose = new PREACTColor(255, 240, 0, 255);
        private static readonly PREACTColor Zaffre = new PREACTColor(0, 20, 168, 255);
        private static readonly PREACTColor ZinnwalditeBrown = new PREACTColor(44, 22, 8, 255);
        private static readonly PREACTColor Zomp = new PREACTColor(57, 167, 142, 255);

		private static readonly PREACTColor[] colors =
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
