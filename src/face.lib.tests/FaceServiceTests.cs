using System;
using Xunit;
using face.lib;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using pelazem.azure.cognitive.face;

namespace face.lib.tests
{
	public class UnitTest1
	{
		[Fact]
		public void ApiUrl_ShouldSet()
		{
			// Arrange
			string expected = Guid.NewGuid().ToString();
			FaceService svc = new FaceService();

			// Act
			svc.ApiUrl = expected;

			// Assert
			Assert.Equal(expected, svc.ApiUrl);
		}

		[Fact]
		public void ApiKey_ShouldSet()
		{
			// Arrange
			string expected = Guid.NewGuid().ToString();
			FaceService svc = new FaceService();

			// Act
			svc.ApiKey = expected;

			// Assert
			Assert.Equal(expected, svc.ApiKey);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void FaceService_CtorShouldThrowWithEmptyOrNullApiUrl(string apiUrl)
		{
			// Arrange
			string apiKey = Guid.NewGuid().ToString();

			// Act
			Action ctor = () => new FaceService(apiUrl, apiKey);

			// Assert
			Assert.ThrowsAny<Exception>(ctor);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void FaceService_CtorShouldThrowWithEmptyOrNullApiKey(string apiKey)
		{
			// Arrange
			string apiUrl = Guid.NewGuid().ToString();

			// Act
			Action ctor = () => new FaceService(apiUrl, apiKey);

			// Assert
			Assert.ThrowsAny<Exception>(ctor);
		}


		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(@"c:\fake\fooFile.jpg")]
		public async Task ProcessImageAtLocal_ShouldReturnResultSuccessFalseWithInvalidFilePath(string filePath)
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			FaceServiceResult result = await svc.ProcessImageAtLocal(filePath);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}

		[Fact]
		public async Task ProcessImageAsByteArray_ShouldReturnResultSuccessFalseWithNullByteArray()
		{
			// Arrange
			byte[] bytes = null;
			FaceService svc = new FaceService();

			// Act
			FaceServiceResult result = await svc.ProcessImageAsByteArray(bytes);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}

		[Fact]
		public async Task ProcessImageAsByteArray_ShouldReturnResultSuccessFalseWithEmptyByteArray()
		{
			// Arrange
			byte[] bytes = { };
			FaceService svc = new FaceService();

			// Act
			FaceServiceResult result = await svc.ProcessImageAsByteArray(bytes);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(@"http://fa.ke/fooFile.jpg")]
		public async Task ProcessImageAtUrl_ShouldReturnResultSuccessFalseWithInvalidUri(string uri)
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			FaceServiceResult result = await svc.ProcessImageAtUrl(uri);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}


		[Fact]
		public async Task GetFaceResult_ShouldReturnResultSuccessFalseWithNullHttpResponse()
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			FaceServiceResult result = await svc.GetFaceResult(null);

			// Assert
			Assert.NotNull(result);
			Assert.False(result.Succeeded);
		}


		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(@"c:\fake\fooFile.jpg")]
		public void GetImageAsByteArray_ShouldReturnNullWithInvalidFilePath(string filePath)
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			byte[] result = svc.GetImageAsByteArray(filePath);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public void GetImagePayload_ShouldReturnNullWithNullByteArray()
		{
			// Arrange
			byte[] bytes = null;
			FaceService svc = new FaceService();

			// Act
			ByteArrayContent result = svc.GetImagePayload(bytes);

			// Assert
			Assert.Null(result);
		}

		[Fact]
		public void GetImagePayload_ShouldReturnNullWithEmptyByteArray()
		{
			// Arrange
			byte[] bytes = { };
			FaceService svc = new FaceService();

			// Act
			ByteArrayContent result = svc.GetImagePayload(bytes);

			// Assert
			Assert.Null(result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		public void GetImageUriContent_ShouldReturnNullWithEmptyOrNullUri(string uri)
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			var result = svc.GetImageUriContent(uri);

			// Assert
			Assert.Null(result);
		}

		[Theory]
		[InlineData(null)]
		[InlineData("")]
		[InlineData(@"c:\fake\fooFile.jpg")]
		public void ValidateFileSize_ShouldReturnFalseWithInvalidFilePath(string filePath)
		{
			// Arrange
			FaceService svc = new FaceService();

			// Act
			bool result = svc.ValidateFileSize(filePath);

			// Assert
			Assert.False(result);
		}

	}
}
