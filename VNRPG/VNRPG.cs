<Project Sdk="Microsoft.NET.Sdk" ToolsVersion="15.0">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.2</TargetFramework>
    <RuntimeFrameworkVersion>2.0.7</RuntimeFrameworkVersion>
    <FileUpgradeFlags>40</FileUpgradeFlags>
    <UpgradeBackupLocation>C:\RAGEMP\server-files\bridge\resources\WiredPlayers\Backup1\</UpgradeBackupLocation>
    <OldToolsVersion>2.0</OldToolsVersion>
    <LangVersion>7.2</LangVersion>
  </PropertyGroup>
  <ItemGroup>
    <Compile Remove="internationalization\**" />
    <EmbeddedResource Remove="internationalization\**" />
    <None Remove="internationalization\**" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="MySql.Data" Version="8.0.15" />
  </ItemGroup>
  <ItemGroup>
    <Reference Include="Bootstrapper">
      <HintPath>..\..\runtime\Bootstrapper.dll</HintPath>
    </Reference>
  </ItemGroup>
  <ItemGroup>
    <Compile Update="messages\administration\AdminRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>AdminRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\description\DescRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>DescRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\error\ErrRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>ErrRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\factions\FacRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>FacRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\general\GenRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>GenRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\information\InfoRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>InfoRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\items\ItemRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>ItemRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\jobs\JobRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>JobRes.resx</DependentUpon>
    </Compile>
    <Compile Update="messages\success\SuccRes.Designer.cs">
      <DesignTime>True</DesignTime>
      <AutoGen>True</AutoGen>
      <DependentUpon>SuccRes.resx</DependentUpon>
    </Compile>
  </ItemGroup>
  <ItemGroup>
    <EmbeddedResource Update="messages\error\ErrRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>ErrRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\description\DescRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>DescRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\description\DescRes.en-GB.resx">
      <LastGenOutput>DescRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\factions\FacRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>FacRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\factions\FacRes.en-GB.resx">
      <LastGenOutput>FacRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\general\GenRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>GenRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\general\GenRes.en-GB.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>GenRes.en-GB.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\information\InfoRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>InfoRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\information\InfoRes.en-GB.resx">
      <LastGenOutput>InfoRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\items\ItemRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>ItemRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\items\ItemRes.en-GB.resx">
      <LastGenOutput>ItemRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\jobs\JobRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>JobRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\jobs\JobRes.en-GB.resx">
      <LastGenOutput>JobRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\administration\AdminRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>AdminRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\administration\AdminRes.en-GB.resx">
      <LastGenOutput>AdminRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\success\SuccRes.resx">
      <Generator>PublicResXFileCodeGenerator</Generator>
      <LastGenOutput>SuccRes.Designer.cs</LastGenOutput>
    </EmbeddedResource>
    <EmbeddedResource Update="messages\success\SuccRes.en-GB.resx">
      <LastGenOutput>SuccRes.en-GB.Designer.cs</LastGenOutput>
      <Generator>PublicResXFileCodeGenerator</Generator>
    </EmbeddedResource>
  </ItemGroup>
</Project>
