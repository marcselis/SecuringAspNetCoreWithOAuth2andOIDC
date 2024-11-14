// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace Marvin.IDP.Pages.Consent;

public class InputModel
{
  public string? Button { get; set; }
#pragma warning disable IDE0028 // Simplify collection initialization, doesn't work if this is applied
  // ReSharper disable once HeapView.ObjectAllocation.Evident
  public IEnumerable<string> ScopesConsented { get; set; } = new List<string>();
#pragma warning restore IDE0028 // Simplify collection initialization
  public bool RememberConsent { get; set; } = true;
  public string? ReturnUrl { get; set; }
  public string? Description { get; set; }
}