using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace twitter_test_server.Controllers {
	[ApiController]
	public class HomeController : Controller {
		private static Dictionary<string, string> requestTokenCallbacks = new Dictionary<string, string>();
		private readonly ILogger<HomeController> logger;
		private readonly IEnumerable<TwitterUser> users = new [] {
			new TwitterUser() {
					Id = "11111",
					Name = "jeffrocams",
					Email = "jeff@jeffcamera.com",
					Verified = false,
					AccessTokenValue = "TEST_ACCESS_TOKEN_VALUE_11111",
					AccessTokenSecret = "TEST_ACCESS_TOKEN_SECRET_11111"
			},
			new TwitterUser() {
					Id = "11112",
					Name = "gfreeman",
					Email = "freeman@blackmesa.com",
					Verified = false,
					AccessTokenValue = "TEST_ACCESS_TOKEN_VALUE_11112",
					AccessTokenSecret = "TEST_ACCESS_TOKEN_SECRET_11112"
			},
			new TwitterUser() {
					Id = "11113",
					Name = "avance",
					Email = "avance@blackmesa.com",
					Verified = false,
					AccessTokenValue = "TEST_ACCESS_TOKEN_VALUE_11113",
					AccessTokenSecret = "TEST_ACCESS_TOKEN_SECRET_11113"
			},
			new TwitterUser() {
					Id = "11114",
					Name = "tt1",
					Email = "tt1@readup.com",
					Verified = false,
					AccessTokenValue = "TEST_ACCESS_TOKEN_VALUE_11114",
					AccessTokenSecret = "TEST_ACCESS_TOKEN_SECRET_11114"
			},
			new TwitterUser() {
					Id = "11115",
					Name = "tt2",
					Email = "tt2@readup.com",
					Verified = false,
					AccessTokenValue = "TEST_ACCESS_TOKEN_VALUE_11115",
					AccessTokenSecret = "TEST_ACCESS_TOKEN_SECRET_11115"
			}
		};
		public HomeController(ILogger<HomeController> logger) {
			this.logger = logger;
		}
		private Token CreateRequestToken() {
			var rng = new Random();
			var randomNumber = rng.Next(10000, 99999);
			return new Token() {
				Value = $"TEST_REQUEST_TOKEN_VALUE_{randomNumber}",
				Secret = $"TEST_REQUEST_TOKEN_SECRET_{randomNumber}"
			};
		}
		private int GenerateRandomId() {
			var rng = new Random();
			return rng.Next(10000, 99999);
		}
		private string GetAuthorizationHeaderValue(string key) => Uri.UnescapeDataString(
			Request
				.Headers["Authorization"]
				.Single()
				.Split("OAuth ")
				.Last()
				.Split(", ")
				.ToDictionary(
					kvp => kvp.Split('=')[0],
					kvp => kvp.Split('=')[1].Trim('"')
				)
				[key]
		);
		private string JsonSerialize(object value) => JsonConvert.SerializeObject(
			value,
			new JsonSerializerSettings() {
				ContractResolver = new DefaultContractResolver() {
					NamingStrategy = new SnakeCaseNamingStrategy()
				}
			}
		);
		// step 1: generate a request token and store the callback
		[HttpPost]
		[Route("oauth/request_token")]
		public IActionResult RequestToken() {
			var requestToken = CreateRequestToken();
			requestTokenCallbacks[requestToken.Value] = GetAuthorizationHeaderValue("oauth_callback");
			return Content($"oauth_token={requestToken.Value}&oauth_token_secret={requestToken.Secret}&oauth_callback_confirmed=true");
		}
		// step 2: return the authorization form
		[HttpGet]
		[Route("oauth/authorize")]
		public IActionResult Authorize(string oauth_token) {
			return View();
		}
		// step 3: process authorization form
		[HttpPost]
		[Route("oauth/authorize")]
		public IActionResult Authorize(
			[FromQuery] string oauth_token,
			[FromForm] string user_name
		) {
			var callbackUrl = new UriBuilder(requestTokenCallbacks[oauth_token]);
			if (!String.IsNullOrWhiteSpace(callbackUrl.Query)) {
				callbackUrl.Query += "&";
			}
			callbackUrl.Query += $"oauth_token={oauth_token}&";
			if (String.IsNullOrWhiteSpace(user_name)) {
				callbackUrl.Query += "denied=true";
			} else {
				var user = users.Single(
					user => user.Name == user_name
				);
				callbackUrl.Query += $"oauth_verifier=TEST_VERIFIER_TOKEN_{user.Id}";
			}
			return Redirect(callbackUrl.Uri.ToString());
		}
		// step 4: return access token
		[HttpPost]
		[Route("oauth/access_token")]
		public IActionResult AccessToken() {
			var user = users.Single(
				user => user.Id == GetAuthorizationHeaderValue("oauth_verifier").Split('_').Last()
			);
			return Content($"oauth_token={user.AccessTokenValue}&oauth_token_secret={user.AccessTokenSecret}&screen_name={user.Name}&user_id={user.Id}");
		}
		// step 5: verify user
		[HttpGet]
		[Route("1.1/account/verify_credentials.json")]
		public IActionResult VerifyCredentials() {
			var user = users.Single(
				user => user.AccessTokenValue == GetAuthorizationHeaderValue("oauth_token")
			);
			return Content(
				JsonSerialize(
					new {
						Name = user.Name,
						Email = user.Email,
						Verified = user.Verified
					}
				)
			);
		}
		// step 6: upload image
		[HttpPost]
		[Route("1.1/media/upload.json")]
		public async Task<IActionResult> Media(
			[FromForm] IFormFile media
		) {
			using (var uploadStream = media.OpenReadStream())
			using (var writeStream = System.IO.File.OpenWrite("temp/" + media.FileName)) {
				await uploadStream.CopyToAsync(writeStream);
				await writeStream.FlushAsync();
			}
			return Content(
				JsonSerialize(
					new {
						MediaId = GenerateRandomId()
					}
				)
			);
		}
		// step 7: tweet
		[HttpPost]
		[Route("1.1/statuses/update.json")]
		public IActionResult Tweet(string status) {
			// Response.StatusCode = 400;
			// return Content(
			// 	JsonSerialize(
			// 		new {
			// 			Errors = new [] {
			// 				new {
			// 					Code = 89,
			// 					Message = "Error"
			// 				}
			// 			}
			// 		}
			// 	)
			// );
			return Content(
				JsonSerialize(
					new {
						Id = GenerateRandomId()
					}
				)
			);
		}
		// aotd bot
		[Route("1.1/users/search.json")]
		public IActionResult Search(string q) {
			return Content(
				JsonSerialize(
					new string[0]
				)
			);
		}
	}
}
